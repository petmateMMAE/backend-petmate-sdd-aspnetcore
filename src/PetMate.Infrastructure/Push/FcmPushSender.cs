using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PetMate.Application.Push;

namespace PetMate.Infrastructure.Push;

/// <summary>
/// Credencial de la cuenta de servicio de Firebase (pm-153#CA-02). Sale solo de la variable <c>FCM_SERVICE_ACCOUNT_JSON</c> (secreto del
/// entorno) y <c>ToString</c> nunca imprime su contenido.
/// </summary>
public sealed record FcmOptions(string ProjectId, string ClientEmail, string PrivateKeyPem)
{
    public const string Variable = "FCM_SERVICE_ACCOUNT_JSON";

    public override string ToString() => nameof(FcmOptions);

    /// <summary>Devuelve <c>null</c> si falta la variable o el JSON no trae <c>project_id</c>, <c>client_email</c> y <c>private_key</c>.</summary>
    public static FcmOptions? TryRead(Func<string, string?> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        if (read(Variable) is not { Length: > 0 } json)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            string? Get(string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            return Get("project_id") is { Length: > 0 } project && Get("client_email") is { Length: > 0 } email && Get("private_key") is { Length: > 0 } key
                ? new FcmOptions(project, email, key)
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>Sin proveedor configurado: no envía nada y no registra nada del token.</summary>
public sealed class DisabledPushSender : IPushSender
{
    public bool IsEnabled => false;

    public Task<PushResult> Send(string deviceToken, PushMessage message, CancellationToken cancellationToken) => Task.FromResult(PushResult.Disabled);
}

/// <summary>
/// Adaptador a Firebase Cloud Messaging por su API HTTP v1, sin paquetes nuevos (pm-153#CA-04): el token OAuth se obtiene firmando
/// un JWT RS256 con la clave de la cuenta de servicio (<see cref="RSA"/> de la biblioteca base) y se guarda hasta poco antes de que
/// venza. FCM no tiene costo (firebase.google.com/products/cloud-messaging, verificado el 2026-11-17). Espera máxima de 5 segundos
/// por llamada; cualquier fallo devuelve <see cref="PushResult.Failed"/> y se registra solo el código de estado, nunca la credencial,
/// el token del dispositivo ni el contenido.
/// </summary>
public sealed partial class FcmPushSender(HttpClient client, FcmOptions options, TimeProvider timeProvider, ILogger<FcmPushSender> logger) : IPushSender, IDisposable
{
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string Scope = "https://www.googleapis.com/auth/firebase.messaging";
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly SemaphoreSlim tokenGate = new(1, 1);
    private string? accessToken;
    private DateTimeOffset accessTokenExpiresAt;

    public bool IsEnabled => true;

    public void Dispose() => tokenGate.Dispose();

    public async Task<PushResult> Send(string deviceToken, PushMessage message, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceToken);
        ArgumentNullException.ThrowIfNull(message);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CallTimeout);
        try
        {
            if (await AccessToken(timeout.Token).ConfigureAwait(false) is not { } bearer)
            {
                return PushResult.Failed;
            }

            var url = $"https://fcm.googleapis.com/v1/projects/{Uri.EscapeDataString(options.ProjectId)}/messages:send";
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(new FcmEnvelope(new FcmMessage(deviceToken, new FcmNotification(message.Title, message.Body)))),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            using var response = await client.SendAsync(request, timeout.Token).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return PushResult.Sent;
            }

            var status = (int)response.StatusCode;
            // 404 UNREGISTERED: el dispositivo ya no existe; el llamador borra el token (pm-152#CA-02).
            if (response.StatusCode == HttpStatusCode.NotFound
                && (await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false)).Contains("UNREGISTERED", StringComparison.Ordinal))
            {
                return PushResult.InvalidToken;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // El token OAuth guardado ya no sirve: se pide otro en el siguiente envío.
                accessToken = null;
            }

            LogRejected(logger, status);
            return PushResult.Failed;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or OperationCanceledException or CryptographicException or JsonException or InvalidOperationException or ArgumentException)
        {
            LogFailed(logger, exception.GetType().Name);
            return PushResult.Failed;
        }
    }

    private async Task<string?> AccessToken(CancellationToken cancellationToken)
    {
        await tokenGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var now = timeProvider.GetUtcNow();
            if (accessToken is not null && now < accessTokenExpiresAt - TimeSpan.FromMinutes(1))
            {
                return accessToken;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["assertion"] = SignedAssertion(now),
                }),
            };
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                LogTokenRejected(logger, (int)response.StatusCode);
                return null;
            }

            var token = await response.Content.ReadFromJsonAsync<OAuthToken>(cancellationToken).ConfigureAwait(false);
            if (token?.AccessToken is not { Length: > 0 } value)
            {
                return null;
            }

            accessToken = value;
            accessTokenExpiresAt = now + TimeSpan.FromSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : 3600);
            return value;
        }
        finally
        {
            tokenGate.Release();
        }
    }

    /// <summary>JWT de la cuenta de servicio firmado con RS256 (válido 1 hora), que se cambia por un token de acceso.</summary>
    private string SignedAssertion(DateTimeOffset now)
    {
        var header = Base64Url(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
        var iat = now.ToUnixTimeSeconds();
        var claims = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
        {
            ["iss"] = options.ClientEmail,
            ["scope"] = Scope,
            ["aud"] = TokenEndpoint,
            ["iat"] = iat,
            ["exp"] = iat + 3600,
        }));
        var unsigned = string.Create(CultureInfo.InvariantCulture, $"{header}.{claims}");
        using var rsa = RSA.Create();
        rsa.ImportFromPem(options.PrivateKeyPem);
        return unsigned + "." + Base64Url(rsa.SignData(Encoding.ASCII.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    [LoggerMessage(EventId = 2201, Level = LogLevel.Warning, Message = "Aviso push no enviado: FCM respondió {Status}.")]
    private static partial void LogRejected(ILogger logger, int status);

    [LoggerMessage(EventId = 2202, Level = LogLevel.Warning, Message = "Aviso push no enviado: fallo o espera agotada ({Kind}).")]
    private static partial void LogFailed(ILogger logger, string kind);

    [LoggerMessage(EventId = 2203, Level = LogLevel.Warning, Message = "Aviso push no enviado: Google rechazó la credencial de la cuenta de servicio ({Status}).")]
    private static partial void LogTokenRejected(ILogger logger, int status);

    private sealed record OAuthToken(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record FcmEnvelope([property: JsonPropertyName("message")] FcmMessage Message);

    private sealed record FcmMessage(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("notification")] FcmNotification Notification);

    private sealed record FcmNotification(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("body")] string Body);
}
