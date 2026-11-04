namespace PetMate.Api.Contracts.Responses;

public sealed class ApiResponse<T>
{
    public ApiResponse(int statusCode, string message, T? data)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public int StatusCode { get; }

    public string Message { get; }

    public T? Data { get; }
}
