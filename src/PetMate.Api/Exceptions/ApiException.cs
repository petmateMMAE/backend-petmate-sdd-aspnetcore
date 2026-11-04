namespace PetMate.Api.Exceptions;

/// <summary>
/// Error controlado con código HTTP 4xx/5xx y mensaje en español seguro para el cliente.
/// El mensaje se devuelve tal cual: nunca debe provenir de excepciones externas.
/// </summary>
public sealed class ApiException : Exception
{
    public ApiException(int statusCode, string message)
        : base(message)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(statusCode, 400);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(statusCode, 599);
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
