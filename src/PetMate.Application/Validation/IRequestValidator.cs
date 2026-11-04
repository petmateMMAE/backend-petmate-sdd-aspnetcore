namespace PetMate.Application.Validation;

/// <summary>
/// Validador de forma/contrato de un request de Application. Se ejecuta antes del handler.
/// Las invariantes de negocio permanecen en Domain; aquí solo se valida la entrada.
/// </summary>
public interface IRequestValidator<in TRequest>
{
    Task<IReadOnlyList<ValidationFailure>> Validate(TRequest request, CancellationToken cancellationToken);
}
