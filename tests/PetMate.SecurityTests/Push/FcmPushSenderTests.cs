using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PetMate.Application.Push;
using PetMate.Infrastructure.Push;

namespace PetMate.SecurityTests.Push;

/// <summary>Adaptador FCM (pm-153): contrato con el proveedor, credencial y errores. Todo con un proveedor falso: no hay llamadas reales.</summary>
public sealed class FcmPushSenderTests
{
    private static readonly PushMessage Message = new("Firulais: vacuna", "Rabia el 2026-12-20");

    private sealed class Harness : IDisposable
    {
        private readonly RSA rsa = RSA.Create(2048);

        public Harness()
        {
            Options = new FcmOptions("proyecto-prueba", "cuenta@proyecto-prueba.iam.example", rsa.ExportPkcs8PrivateKeyPem());
            Handler = new FakeProvider();
            Logs = new CapturingLogger<FcmPushSender>();
            Sender = new FcmPushSender(new HttpClient(Handler), Options, new FixedClock(), Logs);
        }

        public FcmOptions Options { get; }

        public FakeProvider Handler { get; }

        public CapturingLogger<FcmPushSender> Logs { get; }

        public FcmPushSender Sender { get; }

        public RSA PublicKey => rsa;

        public void Dispose()
        {
            Sender.Dispose();
            rsa.Dispose();
        }
    }

    private sealed class FixedClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class FakeProvider : HttpMessageHandler
    {
        public List<(string Url, string Body, string? Auth)> Calls { get; } = [];

        public HttpStatusCode TokenStatus { get; set; } = HttpStatusCode.OK;

        public HttpStatusCode SendStatus { get; set; } = HttpStatusCode.OK;

        public string SendBody { get; set; } = "{\"name\":\"projects/p/messages/1\"}";

        public bool Throw { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Throw)
            {
                throw new HttpRequestException("red caída");
            }

            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Calls.Add((request.RequestUri!.ToString(), body, request.Headers.Authorization?.Parameter));
            return request.RequestUri.Host == "oauth2.googleapis.com"
                ? new HttpResponseMessage(TokenStatus) { Content = new StringContent("{\"access_token\":\"token-de-acceso-de-prueba\",\"expires_in\":3600}", Encoding.UTF8, "application/json") }
                : new HttpResponseMessage(SendStatus) { Content = new StringContent(SendBody, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Lines { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Lines.Add(formatter(state, exception) + exception);
    }

    [Fact]
    public async Task It_sends_the_notification_with_a_bearer_token_obtained_by_signing_a_jwt_with_the_service_account_key()
    {
        using var h = new Harness();

        var result = await h.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None);

        Assert.Equal(PushResult.Sent, result);
        var tokenCall = h.Handler.Calls[0];
        var assertion = Uri.UnescapeDataString(tokenCall.Body.Split("assertion=")[1]);
        var parts = assertion.Split('.');
        var claims = JsonDocument.Parse(Convert.FromBase64String(Pad(parts[1]))).RootElement;
        Assert.Equal("cuenta@proyecto-prueba.iam.example", claims.GetProperty("iss").GetString());
        Assert.Equal("https://www.googleapis.com/auth/firebase.messaging", claims.GetProperty("scope").GetString());
        Assert.True(h.PublicKey.VerifyData(Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]), Convert.FromBase64String(Pad(parts[2])), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

        var send = h.Handler.Calls[1];
        Assert.Equal("https://fcm.googleapis.com/v1/projects/proyecto-prueba/messages:send", send.Url);
        Assert.Equal("token-de-acceso-de-prueba", send.Auth);
        var message = JsonDocument.Parse(send.Body).RootElement.GetProperty("message");
        Assert.Equal("token-de-dispositivo-0123456789", message.GetProperty("token").GetString());
        Assert.Equal("Firulais: vacuna", message.GetProperty("notification").GetProperty("title").GetString());
    }

    [Fact]
    public async Task The_access_token_is_reused_until_it_is_about_to_expire()
    {
        using var h = new Harness();

        await h.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None);
        await h.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None);

        Assert.Equal(1, h.Handler.Calls.Count(c => c.Url.Contains("oauth2", StringComparison.Ordinal)));
        Assert.Equal(2, h.Handler.Calls.Count(c => c.Url.Contains("messages:send", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task A_404_unregistered_is_reported_as_an_invalid_token_so_the_caller_can_delete_it()
    {
        using var h = new Harness();
        h.Handler.SendStatus = HttpStatusCode.NotFound;
        h.Handler.SendBody = "{\"error\":{\"code\":404,\"status\":\"NOT_FOUND\",\"details\":[{\"errorCode\":\"UNREGISTERED\"}]}}";

        Assert.Equal(PushResult.InvalidToken, await h.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None));
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Any_other_provider_error_is_a_failure_that_does_not_throw_and_keeps_the_token(HttpStatusCode status)
    {
        using var h = new Harness();
        h.Handler.SendStatus = status;

        Assert.Equal(PushResult.Failed, await h.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None));
    }

    [Fact]
    public async Task A_rejected_credential_a_network_failure_or_a_bad_key_do_not_throw()
    {
        using var rejected = new Harness();
        rejected.Handler.TokenStatus = HttpStatusCode.BadRequest;
        using var network = new Harness();
        network.Handler.Throw = true;
        using var badKey = new Harness();
        using var brokenSender = new FcmPushSender(new HttpClient(badKey.Handler), new FcmOptions("p", "c@x", "no es una clave"), TimeProvider.System, badKey.Logs);

        Assert.Equal(PushResult.Failed, await rejected.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None));
        Assert.Equal(PushResult.Failed, await network.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None));
        Assert.Equal(PushResult.Failed, await brokenSender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None));
    }

    [Fact]
    public async Task Logs_and_text_never_contain_the_credential_the_access_token_or_the_device_token()
    {
        using var h = new Harness();
        h.Handler.SendStatus = HttpStatusCode.InternalServerError;

        await h.Sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None);

        var everything = string.Join('\n', h.Logs.Lines) + h.Options + new FcmOptions("p", "c@x", "clave-privada-secreta");
        Assert.NotEmpty(h.Logs.Lines);
        Assert.DoesNotContain("PRIVATE KEY", everything, StringComparison.Ordinal);
        Assert.DoesNotContain("clave-privada-secreta", everything, StringComparison.Ordinal);
        Assert.DoesNotContain("token-de-acceso-de-prueba", everything, StringComparison.Ordinal);
        Assert.DoesNotContain("token-de-dispositivo-0123456789", everything, StringComparison.Ordinal);
    }

    [Fact]
    public void The_credential_is_read_only_from_configuration_and_incomplete_json_is_ignored()
    {
        var valid = JsonSerializer.Serialize(new { project_id = "p", client_email = "c@x", private_key = "k" });

        Assert.Equal("p", FcmOptions.TryRead(name => name == "FCM_SERVICE_ACCOUNT_JSON" ? valid : null)!.ProjectId);
        Assert.Null(FcmOptions.TryRead(_ => null));
        Assert.Null(FcmOptions.TryRead(_ => "no es json"));
        Assert.Null(FcmOptions.TryRead(_ => "{\"project_id\":\"p\"}"));
    }

    [Fact]
    public async Task Without_configuration_the_disabled_sender_sends_nothing()
    {
        var sender = new DisabledPushSender();

        Assert.False(sender.IsEnabled);
        Assert.Equal(PushResult.Disabled, await sender.Send("token-de-dispositivo-0123456789", Message, CancellationToken.None));
    }

    private static string Pad(string value)
    {
        var text = value.Replace('-', '+').Replace('_', '/');
        return text.PadRight(text.Length + ((4 - (text.Length % 4)) % 4), '=');
    }
}
