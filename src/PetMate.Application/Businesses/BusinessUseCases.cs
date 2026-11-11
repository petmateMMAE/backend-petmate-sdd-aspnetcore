using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Validation;
using PetMate.Domain.Businesses;

namespace PetMate.Application.Businesses;

/// <summary>Persistencia del negocio afiliado (pm-105). Toda operación va por el dueño de la cuenta.</summary>
public interface IBusinessRepository
{
    /// <summary>Guarda el negocio; <c>false</c> si la cuenta ya tiene uno (índice único por cuenta, pm-105#CA-02).</summary>
    Task<bool> TryAdd(Business business, CancellationToken cancellationToken);

    Task<Business?> FindByOwner(Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Negocio por identificador, para las lecturas públicas y para elegir destinatario de una autorización.</summary>
    Task<Business?> FindById(Guid businessId, CancellationToken cancellationToken);

    /// <summary>Negocios activos dentro del radio, de más cerca a más lejos (pm-116), con filtros de tipo y nombre.</summary>
    Task<NearbyPage> FindNearby(NearbyQuery query, CancellationToken cancellationToken);

    /// <summary>Guarda los cambios del negocio de la cuenta; <c>false</c> si ya no existe.</summary>
    Task<bool> UpdateOwned(Business business, CancellationToken cancellationToken);

    /// <summary>Elimina el negocio de la cuenta; <c>false</c> si no tenía.</summary>
    Task<bool> DeleteByOwner(Guid ownerId, CancellationToken cancellationToken);
}

/// <summary>Catálogo de servicios de un negocio (pm-112 a pm-115). Toda operación va por el negocio de la cuenta.</summary>
public interface IBusinessServiceRepository
{
    Task Add(BusinessService service, CancellationToken cancellationToken);

    Task<long> CountByBusiness(Guid businessId, CancellationToken cancellationToken);

    /// <summary>Orden estable (pm-113#CA-03): más antiguo primero y, a igualdad de fecha, por identificador.</summary>
    Task<IReadOnlyList<BusinessService>> ListByBusiness(Guid businessId, int skip, int take, CancellationToken cancellationToken);

    Task<BusinessService?> FindOwned(Guid serviceId, Guid businessId, CancellationToken cancellationToken);

    Task<bool> UpdateOwned(BusinessService service, CancellationToken cancellationToken);

    Task<bool> DeleteOwned(Guid serviceId, Guid businessId, CancellationToken cancellationToken);

    /// <summary>Borra todos los servicios del negocio (al darlo de baja).</summary>
    Task<long> DeleteByBusiness(Guid businessId, CancellationToken cancellationToken);
}

public enum BusinessStatus
{
    Ok = 0,

    /// <summary>La cuenta no tiene negocio asociado.</summary>
    NoBusiness = 1,

    /// <summary>La cuenta ya tiene un negocio (409).</summary>
    AlreadyHasBusiness = 2,

    /// <summary>Servicio ajeno o inexistente.</summary>
    NotFound = 3,
}

public sealed record BusinessOutcome<T>(BusinessStatus Status, T? Value = default);

public static class BusinessOutcome
{
    public static BusinessOutcome<T> Ok<T>(T value) => new(BusinessStatus.Ok, value);

    public static BusinessOutcome<T> Failed<T>(BusinessStatus status) => new(status);
}

/// <summary>Negocio tal como lo ve su dueño. La cédula profesional solo sale aquí, nunca en lecturas públicas.</summary>
public sealed record BusinessResult(
    Guid Id,
    BusinessType Type,
    string Name,
    string Phone,
    string? WhatsApp,
    string Address,
    string? Description,
    double Latitude,
    double Longitude,
    string? LicenseNumber,
    IReadOnlyDictionary<string, IReadOnlyList<(string Open, string Close)>> Schedule,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static BusinessResult From(Business b)
    {
        ArgumentNullException.ThrowIfNull(b);
        var schedule = b.Schedule.Days
            .OrderBy(d => ((int)d.Key + 6) % 7)
            .ToDictionary(
                d => WeeklySchedule.DayNames.First(n => n.Value == d.Key).Key,
                d => (IReadOnlyList<(string Open, string Close)>)d.Value.Select(r => (r.Open.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), r.Close.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture))).ToArray());
        return new BusinessResult(b.Id, b.Type, b.Name, b.Phone, b.WhatsApp, b.Address, b.Description, b.Latitude, b.Longitude, b.LicenseNumber, schedule, b.CreatedAtUtc, b.UpdatedAtUtc);
    }
}

public sealed record ServiceResult(Guid Id, string Name, string? Description, decimal? Price, DateTime CreatedAtUtc, DateTime UpdatedAtUtc)
{
    public static ServiceResult From(BusinessService s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return new(s.Id, s.Name, s.Description, s.Price, s.CreatedAtUtc, s.UpdatedAtUtc);
    }
}

// ---------- Negocio (pm-107 a pm-110) ----------

public sealed record CreateBusinessCommand(Guid OwnerId, BusinessDetails Details) : ICommand<BusinessOutcome<BusinessResult>>;

public sealed record UpdateBusinessLocationCommand(Guid OwnerId, double? Latitude, double? Longitude) : ICommand<BusinessOutcome<BusinessResult>>;

public sealed record UpdateBusinessCommand(Guid OwnerId, BusinessProfile Profile) : ICommand<BusinessOutcome<BusinessResult>>;

public sealed record DeleteBusinessCommand(Guid OwnerId) : ICommand<BusinessStatus>;

public sealed class CreateBusinessHandler(IBusinessRepository businesses, TimeProvider timeProvider, PublicBusinessCache? publicCache = null)
    : IRequestHandler<CreateBusinessCommand, BusinessOutcome<BusinessResult>>
{
    public async Task<BusinessOutcome<BusinessResult>> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        // La unicidad por cuenta la decide el índice único de Atlas; la lectura previa solo evita trabajo inútil.
        if (await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false) is not null)
        {
            return BusinessOutcome.Failed<BusinessResult>(BusinessStatus.AlreadyHasBusiness);
        }

        var business = Business.Create(request.OwnerId, request.Details, timeProvider.GetUtcNow().UtcDateTime);
        if (!await businesses.TryAdd(business, cancellationToken).ConfigureAwait(false))
        {
            return BusinessOutcome.Failed<BusinessResult>(BusinessStatus.AlreadyHasBusiness);
        }

        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(BusinessResult.From(business));
    }
}

public sealed class UpdateBusinessLocationHandler(IBusinessRepository businesses, TimeProvider timeProvider, PublicBusinessCache? publicCache = null)
    : IRequestHandler<UpdateBusinessLocationCommand, BusinessOutcome<BusinessResult>>
{
    public async Task<BusinessOutcome<BusinessResult>> Handle(UpdateBusinessLocationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessOutcome.Failed<BusinessResult>(BusinessStatus.NoBusiness);
        }

        business.MoveTo(request.Latitude!.Value, request.Longitude!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (!await businesses.UpdateOwned(business, cancellationToken).ConfigureAwait(false))
        {
            return BusinessOutcome.Failed<BusinessResult>(BusinessStatus.NoBusiness);
        }

        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(BusinessResult.From(business));
    }
}

public sealed class UpdateBusinessHandler(IBusinessRepository businesses, TimeProvider timeProvider, PublicBusinessCache? publicCache = null)
    : IRequestHandler<UpdateBusinessCommand, BusinessOutcome<BusinessResult>>
{
    public async Task<BusinessOutcome<BusinessResult>> Handle(UpdateBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessOutcome.Failed<BusinessResult>(BusinessStatus.NoBusiness);
        }

        business.UpdateProfile(request.Profile, timeProvider.GetUtcNow().UtcDateTime);
        if (!await businesses.UpdateOwned(business, cancellationToken).ConfigureAwait(false))
        {
            return BusinessOutcome.Failed<BusinessResult>(BusinessStatus.NoBusiness);
        }

        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(BusinessResult.From(business));
    }
}

/// <summary>
/// Da de baja el negocio de la cuenta (pm-110): primero su catálogo y al final el negocio, de modo que un fallo parcial deja el
/// negocio visible y repetir la baja termina el trabajo. La cuenta y sus mascotas no se tocan.
/// </summary>
public sealed class DeleteBusinessHandler(IBusinessRepository businesses, IBusinessServiceRepository services, PublicBusinessCache? publicCache = null) : IRequestHandler<DeleteBusinessCommand, BusinessStatus>
{
    public async Task<BusinessStatus> Handle(DeleteBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessStatus.NoBusiness;
        }

        try
        {
            await services.DeleteByBusiness(business.Id, cancellationToken).ConfigureAwait(false);
            var deleted = await businesses.DeleteByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
            return deleted ? BusinessStatus.Ok : BusinessStatus.NoBusiness;
        }
        finally
        {
            // Aunque la baja falle a mitad, los servicios ya borrados no deben seguir saliendo de la caché.
            await BusinessCacheInvalidation.Invalidate(publicCache, CancellationToken.None).ConfigureAwait(false);
        }
    }
}

// ---------- Servicios (pm-112 a pm-115) ----------

public sealed record CreateServiceCommand(Guid OwnerId, ServiceDetails Details) : ICommand<BusinessOutcome<ServiceResult>>;

public sealed record ListServicesQuery(Guid OwnerId, int? Page, int? PageSize) : IQuery<BusinessOutcome<PagedResult<ServiceResult>>>;

public sealed record UpdateServiceCommand(Guid OwnerId, Guid ServiceId, ServiceDetails Details) : ICommand<BusinessOutcome<ServiceResult>>;

public sealed record DeleteServiceCommand(Guid OwnerId, Guid ServiceId) : ICommand<BusinessStatus>;

public sealed class CreateServiceHandler(IBusinessRepository businesses, IBusinessServiceRepository services, TimeProvider timeProvider, PublicBusinessCache? publicCache = null)
    : IRequestHandler<CreateServiceCommand, BusinessOutcome<ServiceResult>>
{
    public async Task<BusinessOutcome<ServiceResult>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessOutcome.Failed<ServiceResult>(BusinessStatus.NoBusiness);
        }

        var service = BusinessService.Create(business.Id, request.Details, timeProvider.GetUtcNow().UtcDateTime);
        await services.Add(service, cancellationToken).ConfigureAwait(false);
        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(ServiceResult.From(service));
    }
}

public sealed class ListServicesHandler(IBusinessRepository businesses, IBusinessServiceRepository services)
    : IRequestHandler<ListServicesQuery, BusinessOutcome<PagedResult<ServiceResult>>>
{
    public async Task<BusinessOutcome<PagedResult<ServiceResult>>> Handle(ListServicesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessOutcome.Failed<PagedResult<ServiceResult>>(BusinessStatus.NoBusiness);
        }

        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var total = await services.CountByBusiness(business.Id, cancellationToken).ConfigureAwait(false);
        var skip = ((long)page - 1) * pageSize;
        if (skip >= total || skip > int.MaxValue)
        {
            return BusinessOutcome.Ok(new PagedResult<ServiceResult>([], page, pageSize, total));
        }

        var items = await services.ListByBusiness(business.Id, (int)skip, pageSize, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(new PagedResult<ServiceResult>(items.Select(ServiceResult.From).ToArray(), page, pageSize, total));
    }
}

public sealed class UpdateServiceHandler(IBusinessRepository businesses, IBusinessServiceRepository services, TimeProvider timeProvider, PublicBusinessCache? publicCache = null)
    : IRequestHandler<UpdateServiceCommand, BusinessOutcome<ServiceResult>>
{
    public async Task<BusinessOutcome<ServiceResult>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        var service = business is null ? null : await services.FindOwned(request.ServiceId, business.Id, cancellationToken).ConfigureAwait(false);
        if (service is null)
        {
            // Sin negocio, servicio de otro negocio o inexistente: el mismo resultado (pm-114#CA-02).
            return BusinessOutcome.Failed<ServiceResult>(BusinessStatus.NotFound);
        }

        service.Update(request.Details, timeProvider.GetUtcNow().UtcDateTime);
        if (!await services.UpdateOwned(service, cancellationToken).ConfigureAwait(false))
        {
            return BusinessOutcome.Failed<ServiceResult>(BusinessStatus.NotFound);
        }

        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(ServiceResult.From(service));
    }
}

public sealed class DeleteServiceHandler(IBusinessRepository businesses, IBusinessServiceRepository services, PublicBusinessCache? publicCache = null) : IRequestHandler<DeleteServiceCommand, BusinessStatus>
{
    public async Task<BusinessStatus> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessStatus.NotFound;
        }

        var deleted = await services.DeleteOwned(request.ServiceId, business.Id, cancellationToken).ConfigureAwait(false);
        if (deleted)
        {
            await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        }

        return deleted ? BusinessStatus.Ok : BusinessStatus.NotFound;
    }
}

/// <summary>Invalida las respuestas públicas en caché (pm-121#CA-02). Sin caché configurada no hace nada.</summary>
internal static class BusinessCacheInvalidation
{
    public static Task Invalidate(PublicBusinessCache? cache, CancellationToken cancellationToken)
        => cache is null ? Task.CompletedTask : cache.InvalidateAll(cancellationToken);
}

// ---------- Validadores ----------

internal static class BusinessValidation
{
    private static readonly Dictionary<string, string> Messages = new(StringComparer.Ordinal)
    {
        ["type"] = "El tipo debe ser Veterinary, Store o Grooming.",
        ["name"] = $"El nombre es obligatorio y debe tener de 1 a {Business.MaxNameLength} caracteres.",
        ["phone"] = $"El teléfono es obligatorio y debe tener {PetMate.Domain.Users.User.PhoneDigits} dígitos (se admite +52, espacios y guiones).",
        ["whatsApp"] = $"El WhatsApp debe tener {PetMate.Domain.Users.User.PhoneDigits} dígitos.",
        ["address"] = $"La dirección es obligatoria y debe tener de 1 a {Business.MaxAddressLength} caracteres.",
        ["description"] = $"La descripción no debe exceder {Business.MaxDescriptionLength} caracteres.",
        ["latitude"] = "La latitud debe estar entre -90 y 90.",
        ["longitude"] = "La longitud debe estar entre -180 y 180.",
        ["licenseNumber"] = $"La cédula profesional es obligatoria para una veterinaria y no debe exceder {Business.MaxLicenseLength} caracteres.",
        ["schedule"] = "El horario no es válido: días monday a sunday, horas HH:mm, cierre posterior a la apertura y sin franjas solapadas.",
        ["price"] = $"El precio debe ser mayor o igual que 0, con hasta 2 decimales y no más de {BusinessService.MaxPrice:0}.",
    };

    public static Task<IReadOnlyList<ValidationFailure>> Failures(IEnumerable<string> fields)
        => Task.FromResult<IReadOnlyList<ValidationFailure>>(fields.Select(f => new ValidationFailure(f, Messages.GetValueOrDefault(f, "El valor no es válido."))).ToArray());
}

public sealed class CreateBusinessCommandValidator : IRequestValidator<CreateBusinessCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return BusinessValidation.Failures(request.Details.InvalidFields());
    }
}

public sealed class UpdateBusinessLocationCommandValidator : IRequestValidator<UpdateBusinessLocationCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateBusinessLocationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return BusinessValidation.Failures(Business.InvalidLocationFields(request.Latitude, request.Longitude));
    }
}

public sealed class UpdateBusinessCommandValidator : IRequestValidator<UpdateBusinessCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return BusinessValidation.Failures(request.Profile.InvalidFields());
    }
}

public sealed class CreateServiceCommandValidator : IRequestValidator<CreateServiceCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return BusinessValidation.Failures(request.Details.InvalidFields());
    }
}

public sealed class UpdateServiceCommandValidator : IRequestValidator<UpdateServiceCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return BusinessValidation.Failures(request.Details.InvalidFields());
    }
}

public sealed class ListServicesQueryValidator : IRequestValidator<ListServicesQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListServicesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PaginationConvention.Check(request.Page, request.PageSize));
    }
}
