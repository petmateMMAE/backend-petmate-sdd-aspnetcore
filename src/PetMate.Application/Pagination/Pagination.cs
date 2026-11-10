using PetMate.Application.Validation;

namespace PetMate.Application.Pagination;

/// <summary>
/// Convención de paginación de los listados (pm-047). <c>page</c> empieza en 1; <c>pageSize</c> vale 20 por defecto y 50 como
/// máximo. Un valor fuera de rango no se recorta en silencio: se rechaza con 400 y el nombre del parámetro.
/// </summary>
public static class PaginationConvention
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 50;

    /// <summary>Valida los parámetros recibidos (<c>null</c> = no enviado, se usa el valor por defecto).</summary>
    public static IReadOnlyList<ValidationFailure> Check(int? page, int? pageSize)
    {
        var failures = new List<ValidationFailure>();
        if (page is < 1)
        {
            failures.Add(new ValidationFailure("page", "La página debe ser un número entero mayor o igual que 1."));
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            failures.Add(new ValidationFailure("pageSize", $"El tamaño de página debe estar entre 1 y {MaxPageSize}."));
        }

        return failures;
    }

    public static int ResolvePage(int? page) => page ?? DefaultPage;

    public static int ResolvePageSize(int? pageSize) => pageSize ?? DefaultPageSize;
}

/// <summary>Página de resultados con el total para que el cliente calcule las páginas (pm-047).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)((TotalCount + PageSize - 1) / PageSize);
}
