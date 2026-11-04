using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Exceptions;
using PetMate.Api.Middleware;

namespace PetMate.SecurityTests.Responses;

public sealed class ApiResponseSecurityTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Factory_creates_matching_http_and_wrapper_status_codes()
    {
        var result = ApiResponseFactory.Ok<object?>("Operación correcta.", null);
        var body = Assert.IsType<ApiResponse<object?>>(result.Value);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, body.StatusCode);
        Assert.Null(body.Data);
    }

    [Fact]
    public void Wrapper_serializes_exact_camel_case_properties_and_null_data()
    {
        var response = new ApiResponse<object?>(200, "Operación correcta.", null);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonOptions));

        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task Middleware_hides_unexpected_exception_details()
    {
        const string secret = "never-expose-this-secret";
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException(secret), NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var payload = await reader.ReadToEndAsync();

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.DoesNotContain(secret, payload, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("stack", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Middleware_maps_expected_api_exception()
    {
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ApiException(StatusCodes.Status404NotFound, "Recurso no encontrado."), NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await JsonSerializer.DeserializeAsync<ApiResponse<object?>>(context.Response.Body, JsonOptions);

        Assert.NotNull(body);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, body.StatusCode);
        Assert.Equal("Recurso no encontrado.", body.Message);
        Assert.Null(body.Data);
    }
}
