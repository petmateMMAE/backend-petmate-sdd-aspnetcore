# Plan — pm-041 pet-list

## Enfoque
- `GET /api/v1/pets/get-all?page&pageSize` en `PetsController` (política `RequireOwner`): envía `ListMyPetsQuery(OwnerId del token, page, pageSize)`; el propietario sale solo del claim `sub` y cualquier `ownerId` en la consulta se ignora.
- `ListMyPetsHandler` cuenta (`CountByOwner`) y lista (`ListByOwner`, de la más reciente a la más antigua) con filtro de propietario en la consulta de Atlas; una página fuera de rango devuelve `items` vacío sin consultar. El índice `ix_pets_ownerId_createdAtUtc` sirve la consulta (pm-039).
- Paginación según pm-047. Sin paquetes nuevos.
