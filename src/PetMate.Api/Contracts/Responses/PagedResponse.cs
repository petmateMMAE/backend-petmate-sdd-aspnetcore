namespace PetMate.Api.Contracts.Responses;

/// <summary>Forma estándar de los listados paginados dentro de <c>data</c> (pm-047).</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount, int TotalPages);
