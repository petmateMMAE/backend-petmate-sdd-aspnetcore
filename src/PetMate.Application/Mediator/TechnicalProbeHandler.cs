namespace PetMate.Application.Mediator;

/// <summary>
/// Handler técnico explícito que sustenta las pruebas de composición T006/T007;
/// no representa una funcionalidad de producto.
/// </summary>
public sealed class TechnicalProbeHandler : IRequestHandler<TechnicalProbeQuery, string>
{
    public Task<string> Handle(TechnicalProbeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult("mediated");
    }
}
