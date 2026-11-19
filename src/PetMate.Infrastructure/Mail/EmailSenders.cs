using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PetMate.Application.Authentication;

namespace PetMate.Infrastructure.Mail;

/// <summary>
/// Configuración del envío de correo (pm-088). La credencial sale solo de variables de entorno seguras y nunca se registra.
/// Proveedor elegido en el plan: Resend (plan gratuito: 3 000 correos al mes y 100 al día, verificado el 2026-11-09 en
/// resend.com/pricing). Exige un dominio de envío verificado (SPF y DKIM).
/// </summary>
public sealed record EmailOptions(string ApiKey, string From, Uri ResetUrlBase, Uri? DeletionUrlBase = null)
{
    public const string ApiKeyVariable = "EMAIL_API_KEY";
    public const string FromVariable = "EMAIL_FROM";
    public const string ResetUrlBaseVariable = "PASSWORD_RESET_URL_BASE";

    /// <summary>Opcional (pm-164): URL HTTPS a la que se añade <c>?token=</c> en el correo de eliminación de cuenta. Sin ella ese correo no se envía.</summary>
    public const string DeletionUrlBaseVariable = "ACCOUNT_DELETION_URL_BASE";

    public override string ToString() => nameof(EmailOptions);

    /// <summary>Devuelve <c>null</c> si falta alguna variable o la URL base no es HTTPS: entonces no se envían correos.</summary>
    public static EmailOptions? TryRead(Func<string, string?> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        var key = read(ApiKeyVariable);
        var from = read(FromVariable);
        var baseUrl = read(ResetUrlBaseVariable);
        return !string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(from)
            && Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? new EmailOptions(key.Trim(), from.Trim(), uri, Uri.TryCreate(read(DeletionUrlBaseVariable), UriKind.Absolute, out var deletion) && deletion.Scheme == Uri.UriSchemeHttps ? deletion : null)
            : null;
    }
}

/// <summary>Sin proveedor configurado: no envía nada y no registra el enlace ni el correo.</summary>
public sealed partial class DisabledEmailSender(ILogger<DisabledEmailSender> logger) : IEmailSender
{
    public Task<bool> SendPasswordReset(string toEmail, string resetToken, CancellationToken cancellationToken)
    {
        LogDisabled(logger);
        return Task.FromResult(false);
    }

    public Task<bool> SendAccountDeletion(string toEmail, string deletionToken, CancellationToken cancellationToken)
    {
        LogDisabled(logger);
        return Task.FromResult(false);
    }

    [LoggerMessage(EventId = 2101, Level = LogLevel.Warning, Message = "Correo no enviado: el proveedor de correo no está configurado.")]
    private static partial void LogDisabled(ILogger logger);
}

/// <summary>
/// Adaptador a la API HTTP de Resend (sin paquetes nuevos). Espera máxima de 5 segundos; cualquier fallo devuelve <c>false</c>
/// y se registra solo el código de estado, nunca la credencial, el destinatario ni el enlace.
/// </summary>
public sealed partial class ResendEmailSender(HttpClient client, EmailOptions options, ILogger<ResendEmailSender> logger) : IEmailSender
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public Task<bool> SendPasswordReset(string toEmail, string resetToken, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetToken);
        var link = new UriBuilder(options.ResetUrlBase) { Query = "token=" + Uri.EscapeDataString(resetToken) }.Uri;
        return Send(new ResendMessage(
            options.From,
            [toEmail],
            "Restablece tu contraseña de PetMate",
            $"<p>Recibimos una solicitud para restablecer tu contraseña de PetMate.</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(link.AbsoluteUri)}\">Crear una contraseña nueva</a></p><p>El enlace vence en 1 hora y solo se puede usar una vez. Si no lo pediste, ignora este correo.</p>"), cancellationToken);
    }

    public Task<bool> SendAccountDeletion(string toEmail, string deletionToken, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(deletionToken);
        if (options.DeletionUrlBase is null)
        {
            // Sin URL de confirmación no hay enlace que enviar: no se manda nada.
            return Task.FromResult(false);
        }

        var link = new UriBuilder(options.DeletionUrlBase) { Query = "token=" + Uri.EscapeDataString(deletionToken) }.Uri;
        return Send(new ResendMessage(
            options.From,
            [toEmail],
            "Confirma la eliminación de tu cuenta de PetMate",
            $"<p>Recibimos una solicitud para eliminar tu cuenta de PetMate y todos tus datos.</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(link.AbsoluteUri)}\">Confirmar la eliminación de mi cuenta</a></p><p>El enlace vence en 1 hora y solo se puede usar una vez. La eliminación no se puede deshacer. Si no lo pediste, ignora este correo y tu cuenta seguirá igual.</p>"), cancellationToken);
    }

    private async Task<bool> Send(ResendMessage payload, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails") { Content = JsonContent.Create(payload) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
            using var response = await client.SendAsync(request, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                LogRejected(logger, status);
                return false;
            }

            return true;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            var kind = exception.GetType().Name;
            LogFailed(logger, kind);
            return false;
        }
    }

    [LoggerMessage(EventId = 2102, Level = LogLevel.Warning, Message = "Correo no enviado: el proveedor respondió {Status}.")]
    private static partial void LogRejected(ILogger logger, int status);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Warning, Message = "Correo no enviado: fallo o espera agotada del proveedor ({Kind}).")]
    private static partial void LogFailed(ILogger logger, string kind);

    private sealed record ResendMessage(
        [property: System.Text.Json.Serialization.JsonPropertyName("from")] string From,
        [property: System.Text.Json.Serialization.JsonPropertyName("to")] string[] To,
        [property: System.Text.Json.Serialization.JsonPropertyName("subject")] string Subject,
        [property: System.Text.Json.Serialization.JsonPropertyName("html")] string Html);
}
