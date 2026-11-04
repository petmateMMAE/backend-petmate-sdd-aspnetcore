using PetMate.Application.Validation;

namespace PetMate.Application.Mediator;

public interface IRequestInvoker
{
    Type RequestType { get; }

    Type ResponseType { get; }

    bool Matches(Type requestType, Type responseType);

    Task<object?> Invoke(object request, CancellationToken cancellationToken);
}

/// <summary>
/// Invocador explícito de un request. Ejecuta todos los validadores registrados antes del handler
/// y acumula los errores; si hay alguno, lanza <see cref="RequestValidationException"/> sin invocar al handler.
/// </summary>
public sealed class RequestInvoker<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    IEnumerable<IRequestValidator<TRequest>>? validators = null) : IRequestInvoker
    where TRequest : IRequest<TResponse>
{
    private readonly IRequestValidator<TRequest>[] validators = validators?.ToArray() ?? [];

    public Type RequestType => typeof(TRequest);

    public Type ResponseType => typeof(TResponse);

    public bool Matches(Type requestType, Type responseType)
        => requestType == typeof(TRequest) && responseType == typeof(TResponse);

    public async Task<object?> Invoke(object request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var typedRequest = (TRequest)request;

        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            failures.AddRange(await validator.Validate(typedRequest, cancellationToken).ConfigureAwait(false));
        }

        if (failures.Count > 0)
        {
            throw new RequestValidationException(failures);
        }

        return await handler.Handle(typedRequest, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class MediatorRegistry
{
    private readonly Dictionary<(Type Request, Type Response), IRequestInvoker> invokers;

    public MediatorRegistry(IEnumerable<IRequestInvoker> invokers)
    {
        ArgumentNullException.ThrowIfNull(invokers);
        this.invokers = invokers.ToDictionary(
            static invoker => (invoker.RequestType, invoker.ResponseType),
            static invoker => invoker);
    }

    public bool TryGet(Type requestType, Type responseType, out IRequestInvoker? invoker)
        => invokers.TryGetValue((requestType, responseType), out invoker);
}

public sealed class MediatorDispatcher(MediatorRegistry registry) : IMediator
{
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGet(request.GetType(), typeof(TResponse), out var invoker))
        {
            throw new InvalidOperationException("No mediator invoker is registered for this request.");
        }

        var result = await invoker!.Invoke(request, cancellationToken).ConfigureAwait(false);
        return (TResponse)result!;
    }
}
