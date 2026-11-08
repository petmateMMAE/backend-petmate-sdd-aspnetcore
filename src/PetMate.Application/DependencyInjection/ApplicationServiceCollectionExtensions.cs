using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetMate.Application.Authentication;
using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Pets;
using PetMate.Application.Security;
using PetMate.Application.Users;
using PetMate.Application.Validation;
using PetMate.Domain.Sessions;

namespace PetMate.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRequestHandler<TechnicalProbeQuery, string, TechnicalProbeHandler>();
        services.AddAuthentication();
        services.AddUserProfile();
        services.AddPets();
        services.TryAddSingleton<MediatorRegistry>();
        services.AddScoped<IMediator, MediatorDispatcher>();
        services.AddScoped<ISender, MediatorDispatcher>();
        return services;
    }

    /// <summary>Casos de uso de autenticación (T018–T021): Register, Login, Refresh y Logout con sus validadores.</summary>
    private static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(SessionPolicy.Default);
        services.AddSingleton<AuthTokenIssuer>();
        // Hashing Argon2id acotado: tantos a la vez como núcleos y el doble en cola; lo demás se rechaza con 429 (pm-034).
        services.TryAddSingleton<IPasswordHashingLimiter>(new BoundedPasswordHashingLimiter(Environment.ProcessorCount, Environment.ProcessorCount * 2));

        services.AddRequestHandler<RegisterCommand, AuthResult<AuthTokensResult>, RegisterHandler>();
        services.AddRequestHandler<LoginCommand, AuthResult<AuthTokensResult>, LoginHandler>();
        services.AddRequestHandler<RefreshCommand, AuthResult<AuthTokensResult>, RefreshHandler>();
        services.AddRequestHandler<LogoutCommand, AuthResult<Unit>, LogoutHandler>();
        services.AddRequestValidator<RegisterCommand, RegisterCommandValidator>();
        services.AddRequestValidator<LoginCommand, LoginCommandValidator>();
        services.AddRequestValidator<RefreshCommand, RefreshCommandValidator>();
        services.AddRequestValidator<LogoutCommand, LogoutCommandValidator>();
        return services;
    }

    /// <summary>Perfil propio (pm-037, pm-038): consulta y actualización del usuario autenticado.</summary>
    private static IServiceCollection AddUserProfile(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddRequestHandler<GetMyProfileQuery, UserProfileResult?, GetMyProfileHandler>();
        services.AddRequestHandler<UpdateMyProfileCommand, UserProfileResult?, UpdateMyProfileHandler>();
        services.AddRequestValidator<UpdateMyProfileCommand, UpdateMyProfileCommandValidator>();
        return services;
    }

    /// <summary>Mascotas del Owner (pm-040, pm-042, pm-043, pm-044): crear, detalle, actualizar y eliminar.</summary>
    private static IServiceCollection AddPets(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddRequestHandler<CreatePetCommand, CreatePetOutcome, CreatePetHandler>();
        services.AddRequestHandler<GetMyPetQuery, PetResult?, GetMyPetHandler>();
        services.AddRequestHandler<ListMyPetsQuery, PagedResult<PetResult>, ListMyPetsHandler>();
        services.AddRequestValidator<ListMyPetsQuery, ListMyPetsQueryValidator>();
        services.AddRequestHandler<UpdateMyPetCommand, PetResult?, UpdateMyPetHandler>();
        services.AddRequestHandler<DeleteMyPetCommand, bool, DeleteMyPetHandler>();
        services.AddRequestValidator<CreatePetCommand, CreatePetCommandValidator>();
        services.AddRequestValidator<UpdateMyPetCommand, UpdateMyPetCommandValidator>();
        return services;
    }

    /// <summary>
    /// Registra de forma explícita un handler y su invocador, que incorpora los validadores
    /// <see cref="IRequestValidator{TRequest}"/> registrados para el mismo request.
    /// </summary>
    public static IServiceCollection AddRequestHandler<TRequest, TResponse, THandler>(this IServiceCollection services)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRequestHandler<TRequest, TResponse>, THandler>();
        services.AddSingleton<IRequestInvoker>(provider => new RequestInvoker<TRequest, TResponse>(
            provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>(),
            provider.GetServices<IRequestValidator<TRequest>>()));
        return services;
    }

    /// <summary>Registra un validador de request; pueden existir varios por request.</summary>
    public static IServiceCollection AddRequestValidator<TRequest, TValidator>(this IServiceCollection services)
        where TValidator : class, IRequestValidator<TRequest>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRequestValidator<TRequest>, TValidator>();
        return services;
    }
}
