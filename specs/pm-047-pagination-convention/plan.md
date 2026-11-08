# Plan — pm-047 pagination-convention

## Enfoque
- `PaginationConvention` (`PetMate.Application.Pagination`) concentra los valores aprobados (2026-11-08): `page` desde 1, `pageSize` 20 por defecto y 50 como máximo; `PagedResult<T>` lleva `Items`, `Page`, `PageSize`, `TotalCount` y `TotalPages`.
- Los parámetros son `int?` en el controlador: un valor no numérico lo rechaza el binding de MVC y uno fuera de rango el validador del caso de uso; ambos terminan en 400 con el Response Wrapper y el nombre del parámetro (nunca se recorta en silencio).
- El contrato HTTP es `PagedResponse<T>` (`items`, `page`, `pageSize`, `totalCount`, `totalPages`) y está documentado en `docs/api/endpoints.md`. Cualquier listado futuro reutiliza la convención.
- Sin paquetes nuevos.
