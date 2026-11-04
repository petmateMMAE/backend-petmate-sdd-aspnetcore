using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Exceptions;
using PetMate.Api.Middleware;
using PetMate.Application.Validation;

namespace PetMate.SecurityTests.Validation;

public sealed class ErrorBoundsSecurityTests
{
    [Fact]
    public void BoundErrors_limits_count_and_text_length()
    {
        var input = Enumerable.Range(0, 500).Select(i => ($"f{i}", new string('x', 5000)));

        var result = ApiResponseFactory.BoundErrors(input);

        Assert.Equal(ApiResponseFactory.MaxErrors, result.Length);
        Assert.All(result, item => Assert.Equal(ApiResponseFactory.MaxTextLength, item.Message.Length));
    }

    [Theory]
    [InlineData(200)]
    [InlineData(399)]
    [InlineData(600)]
    public void ApiException_rejects_non_error_status_codes(int code)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new ApiException(code, "x"));

    [Fact]
    public async Task Middleware_maps_bad_http_request_to_client_error_without_framework_message()
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new BadHttpRequestException("detalle-interno-kestrel", StatusCodes.Status413PayloadTooLarge),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var payload = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.DoesNotContain("detalle-interno-kestrel", payload, StringComparison.Ordinal);
        Assert.Contains("\"statusCode\":413", payload, StringComparison.Ordinal);
    }

    [Fact]
    public void RequestValidationException_exposes_failures()
    {
        var exception = new RequestValidationException([new ValidationFailure("a", "b")]);

        Assert.Single(exception.Failures);
    }
}
