using PetMate.Domain.Common;

namespace PetMate.Domain.Businesses;

/// <summary>Datos de un servicio del catálogo (pm-112): nombre obligatorio, descripción y precio opcionales. Sin existencias ni ventas.</summary>
public sealed record ServiceDetails(string? Name, string? Description, decimal? Price)
{
    public IReadOnlyList<string> InvalidFields()
    {
        var fields = new List<string>();
        if (!Business.IsValidText(Name, BusinessService.MaxNameLength, required: true))
        {
            fields.Add("name");
        }

        if (!Business.IsValidText(Description, BusinessService.MaxDescriptionLength, required: false))
        {
            fields.Add("description");
        }

        if (Price is { } price && (price < 0m || price > BusinessService.MaxPrice || decimal.Round(price, 2) != price))
        {
            fields.Add("price");
        }

        return fields;
    }
}

/// <summary>
/// Servicio del catálogo de un negocio (pm-112). El precio es opcional: sin precio es nulo y nunca 0. No hay inventario ni ventas.
/// </summary>
public sealed class BusinessService
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 300;
    public const decimal MaxPrice = 1_000_000m;

    private BusinessService(Guid id, Guid businessId, ServiceDetails details, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Id = id;
        BusinessId = businessId;
        CreatedAtUtc = createdAtUtc;
        Apply(details);
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public Guid BusinessId { get; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal? Price { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static BusinessService Create(Guid businessId, ServiceDetails details, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        return details.InvalidFields().Count == 0
            ? new BusinessService(Guid.CreateVersion7(), Guard.NotEmpty(businessId, nameof(businessId)), details, now, now)
            : throw new DomainException("Los datos del servicio no son válidos.");
    }

    public void Update(ServiceDetails details, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (details.InvalidFields().Count != 0)
        {
            throw new DomainException("Los datos del servicio no son válidos.");
        }

        Apply(details);
        UpdatedAtUtc = now;
    }

    /// <summary>Rehidrata un servicio persistido. Interno: Infrastructure y pruebas.</summary>
    internal static BusinessService Restore(Guid id, Guid businessId, ServiceDetails details, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var updated = Guard.Utc(updatedAtUtc, nameof(updatedAtUtc));
        return details.InvalidFields().Count == 0 && updated >= created
            ? new BusinessService(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(businessId, nameof(businessId)), details, created, updated)
            : throw new DomainException("El servicio guardado no es válido.");
    }

    private void Apply(ServiceDetails details)
    {
        Name = details.Name!.Trim();
        Description = string.IsNullOrWhiteSpace(details.Description) ? null : details.Description.Trim();
        Price = details.Price;
    }
}
