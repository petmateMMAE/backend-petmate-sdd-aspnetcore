using PetMate.ContractTests.Support;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Exceptions;
using PetMate.Application.DependencyInjection;
using PetMate.Application.Mediator;
using PetMate.Application.Validation;

namespace PetMate.ContractTests.Responses;

[Collection(ApiEnvironmentGroup.Name)]
public sealed class ResponseWrapperHttpTests
{
    private static readonly string[] WrapperProperties = ["data", "message", "statusCode"];

    [Fact]
    public async Task Success_returns_exact_wrapper_with_matching_status()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-wrapper/ok");

        await AssertWrapperAsync(response, HttpStatusCode.OK, expectNullData: false);
    }

    [Fact]
    public async Task Success_with_null_data_serializes_data_null()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-wrapper/null-data");

        await AssertWrapperAsync(response, HttpStatusCode.OK, expectNullData: true);
    }

    [Theory]
    [InlineData("/api/v1/test-wrapper/unauthorized", HttpStatusCode.Unauthorized)]
    [InlineData("/api/v1/test-wrapper/forbidden", HttpStatusCode.Forbidden)]
    [InlineData("/api/v1/test-wrapper/not-found", HttpStatusCode.NotFound)]
    [InlineData("/api/v1/test-wrapper/does-not-exist", HttpStatusCode.NotFound)]
    public async Task Bodyless_status_codes_are_wrapped_in_spanish(string path, HttpStatusCode expected)
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path);

        var message = await AssertWrapperAsync(response, expected, expectNullData: true);
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public async Task Wrong_method_returns_wrapped_405()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.DeleteAsync("/api/v1/test-wrapper/ok");

        await AssertWrapperAsync(response, HttpStatusCode.MethodNotAllowed, expectNullData: true);
    }

    [Fact]
    public async Task Controlled_api_exception_keeps_status_and_message()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-wrapper/controlled-error");

        var message = await AssertWrapperAsync(response, HttpStatusCode.Conflict, expectNullData: true);
        Assert.Equal("Conflicto controlado de prueba.", message);
    }

    [Fact]
    public async Task Unexpected_exception_returns_500_without_internal_details()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-wrapper/boom");
        var raw = await response.Content.ReadAsStringAsync();

        var message = await AssertWrapperAsync(response, HttpStatusCode.InternalServerError, expectNullData: true);
        Assert.Equal(ApiResponseMessages.InternalError, message);
        Assert.DoesNotContain("super-secret-connection-string", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", raw, StringComparison.Ordinal);
        Assert.DoesNotContain(" at ", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("PetMate.", raw, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_body_binding_returns_wrapped_400_with_generic_spanish_errors()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent("{\"age\":\"no-es-numero\"", System.Text.Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/api/v1/test-wrapper/binding", content);
        var raw = await response.Content.ReadAsStringAsync();

        await AssertWrapperAsync(response, HttpStatusCode.BadRequest, expectNullData: false, raw);
        Assert.DoesNotContain("System.", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("Path:", raw, StringComparison.Ordinal);
        Assert.Contains("El valor del campo no es válido.", raw, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_request_through_mediator_returns_wrapped_400_and_skips_handler()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();
        using var content = JsonContent("{\"name\":\"\"}");

        using var response = await client.PostAsync("/api/v1/test-wrapper/validated", content);
        var raw = await response.Content.ReadAsStringAsync();

        await AssertWrapperAsync(response, HttpStatusCode.BadRequest, expectNullData: false, raw);
        using var document = JsonDocument.Parse(raw);
        var errors = document.RootElement.GetProperty("data").GetProperty("errors");
        Assert.Equal(2, errors.GetArrayLength());
        Assert.Equal("name", errors[0].GetProperty("field").GetString());
        Assert.Contains("obligatorio", raw, StringComparison.Ordinal);
        Assert.Equal(0, factory.Services.GetRequiredService<HandlerProbe>().Calls);
    }

    [Fact]
    public async Task Valid_request_through_mediator_executes_handler_and_returns_wrapper()
    {
        using var factory = new WrapperApiFactory();
        using var client = factory.CreateClient();
        using var content = JsonContent("{\"name\":\"Luna\"}");

        using var response = await client.PostAsync("/api/v1/test-wrapper/validated", content);

        await AssertWrapperAsync(response, HttpStatusCode.OK, expectNullData: false);
        Assert.Equal(1, factory.Services.GetRequiredService<HandlerProbe>().Calls);
    }

    private static StringContent JsonContent(string json)
        => new(json, System.Text.Encoding.UTF8, "application/json");

    private static async Task<string?> AssertWrapperAsync(
        HttpResponseMessage response,
        HttpStatusCode expected,
        bool expectNullData,
        string? raw = null)
    {
        raw ??= await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, response.StatusCode);
        Assert.StartsWith("application/json", response.Content.Headers.ContentType?.MediaType, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;
        Assert.Equal(WrapperProperties, root.EnumerateObject().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal));
        Assert.Equal((int)expected, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.String, root.GetProperty("message").ValueKind);
        if (expectNullData)
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
        }

        return root.GetProperty("message").GetString();
    }

    public sealed class HandlerProbe
    {
        public int Calls { get; set; }
    }

    private sealed class WrapperApiFactory : IsolatedApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(WrapperTestController).Assembly);
                services.AddSingleton<HandlerProbe>();
                services.AddRequestHandler<EchoCommand, string, EchoHandler>();
                services.AddRequestValidator<EchoCommand, EchoNameRequiredValidator>();
                services.AddRequestValidator<EchoCommand, EchoNameLengthValidator>();
            });
        }
    }

    public sealed record EchoCommand(string? Name) : ICommand<string>;

    public sealed class EchoHandler(HandlerProbe probe) : IRequestHandler<EchoCommand, string>
    {
        public Task<string> Handle(EchoCommand request, CancellationToken cancellationToken)
        {
            probe.Calls++;
            return Task.FromResult(request.Name ?? string.Empty);
        }
    }

    public sealed class EchoNameRequiredValidator : IRequestValidator<EchoCommand>
    {
        public Task<IReadOnlyList<ValidationFailure>> Validate(EchoCommand request, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ValidationFailure>>(
                string.IsNullOrWhiteSpace(request.Name) ? [new ValidationFailure("name", "El nombre es obligatorio.")] : []);
    }

    public sealed class EchoNameLengthValidator : IRequestValidator<EchoCommand>
    {
        public Task<IReadOnlyList<ValidationFailure>> Validate(EchoCommand request, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ValidationFailure>>(
                (request.Name ?? string.Empty).Length < 2
                    ? [new ValidationFailure("name", "El nombre debe tener al menos 2 caracteres.")]
                    : []);
    }

    public sealed record BindingBody(int Age);
}


#pragma warning disable CA1822 // MVC no enruta acciones static.
[ApiController]
[AllowAnonymous] // Controlador de prueba de contrato del wrapper: con la política de respaldo (pm-033) debe ser público de forma explícita.
[Route("api/v1/test-wrapper")]
public sealed class WrapperTestController(ISender sender) : ControllerBase
{
    [HttpGet("ok")]
    public ObjectResult Ok_() => ApiResponseFactory.Ok("Operación correcta.", new { value = 1 });

    [HttpGet("null-data")]
    public ObjectResult NullData() => ApiResponseFactory.Ok<object?>("Operación correcta.", null);

    [HttpGet("unauthorized")]
    public IActionResult Unauthorized_() => StatusCode(StatusCodes.Status401Unauthorized);

    [HttpGet("forbidden")]
    public IActionResult Forbidden_() => StatusCode(StatusCodes.Status403Forbidden);

    [HttpGet("not-found")]
    public IActionResult NotFound_() => StatusCode(StatusCodes.Status404NotFound);

    [HttpGet("controlled-error")]
    public IActionResult ControlledError()
        => throw new ApiException(StatusCodes.Status409Conflict, "Conflicto controlado de prueba.");

    [HttpGet("boom")]
    public IActionResult Boom()
        => throw new InvalidOperationException("super-secret-connection-string");

    [HttpPost("binding")]
    public ObjectResult Binding([FromBody] ResponseWrapperHttpTests.BindingBody body) => ApiResponseFactory.Ok("Operación correcta.", body);

    [HttpPost("validated")]
    public async Task<ObjectResult> Validated([FromBody] ResponseWrapperHttpTests.EchoCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return ApiResponseFactory.Ok("Operación correcta.", result);
    }
}
#pragma warning restore CA1822
