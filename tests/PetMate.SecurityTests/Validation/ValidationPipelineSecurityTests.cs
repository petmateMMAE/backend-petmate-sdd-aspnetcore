using PetMate.Infrastructure.DependencyInjection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Middleware;
using PetMate.Application.DependencyInjection;
using PetMate.Application.Mediator;
using PetMate.Application.Validation;

namespace PetMate.SecurityTests.Validation;

public sealed class ValidationPipelineSecurityTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Di_registration_wires_validators_before_handler()
    {
        var probe = new Probe();
        var services = new ServiceCollection();
        services.AddSingleton(probe);
        services.AddLogging().AddApplication().AddInfrastructure(PetMate.SecurityTests.Support.TestConfiguration.Valid());
        services.AddRequestHandler<PingCommand, string, PingHandler>();
        services.AddRequestValidator<PingCommand, PingValidator>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<RequestValidationException>(() => sender.Send(new PingCommand("")));
        Assert.Equal(0, probe.Calls);

        var result = await sender.Send(new PingCommand("ok"));
        Assert.Equal("ok", result);
        Assert.Equal(1, probe.Calls);
    }

    [Fact]
    public async Task Middleware_maps_validation_exception_to_400_wrapper_with_errors()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new RequestValidationException([
                new ValidationFailure("name", "El nombre es obligatorio."),
                new ValidationFailure("age", "La edad es obligatoria."),
            ]),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        using var document = JsonDocument.Parse(body);
        Assert.Equal(400, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(ApiResponseMessages.ValidationFailed, document.RootElement.GetProperty("message").GetString());
        Assert.Equal(2, document.RootElement.GetProperty("data").GetProperty("errors").GetArrayLength());
    }

    [Fact]
    public async Task Middleware_does_not_write_when_client_cancelled()
    {
        using var cts = new CancellationTokenSource();
        var context = CreateContext();
        context.RequestAborted = cts.Token;
        var middleware = new ExceptionHandlingMiddleware(
            _ =>
            {
                cts.Cancel();
                throw new OperationCanceledException(cts.Token);
            },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public void Status_messages_are_in_spanish_and_never_empty()
    {
        foreach (var code in new[] { 400, 401, 403, 404, 405, 409, 415, 429, 500, 503, 418 })
        {
            var message = ApiResponseMessages.ForStatusCode(code);
            Assert.False(string.IsNullOrWhiteSpace(message));
        }

        Assert.Equal(ApiResponseMessages.InternalError, ApiResponseMessages.ForStatusCode(502));
    }

    private static DefaultHttpContext CreateContext()
        => new() { Response = { Body = new MemoryStream() } };

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private sealed class Probe
    {
        public int Calls { get; set; }
    }

    private sealed record PingCommand(string Value) : ICommand<string>;

    private sealed class PingHandler(Probe probe) : IRequestHandler<PingCommand, string>
    {
        public Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            probe.Calls++;
            return Task.FromResult(request.Value);
        }
    }

    private sealed class PingValidator : IRequestValidator<PingCommand>
    {
        public Task<IReadOnlyList<ValidationFailure>> Validate(PingCommand request, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ValidationFailure>>(
                request.Value.Length == 0 ? [new ValidationFailure("value", "El valor es obligatorio.")] : []);
    }
}
