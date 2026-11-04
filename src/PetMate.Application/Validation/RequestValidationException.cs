namespace PetMate.Application.Validation;

/// <summary>Se lanza cuando uno o más validadores rechazan un request; el handler no se ejecuta.</summary>
public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IReadOnlyList<ValidationFailure> failures)
        : base("La solicitud contiene datos inválidos.")
    {
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures;
    }

    public IReadOnlyList<ValidationFailure> Failures { get; }
}
