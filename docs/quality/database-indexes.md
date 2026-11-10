# Inventario de índices y consultas críticas (pm-052 / T063)

> Fuente: `MongoIndexInitializer`. Evidencia `explain` (2026-11-08): `QueryPlanIntegrationTests`, contra la base Atlas de pruebas con los índices reales.

## Índices (CA-01)
| Colección | Índice | Campos | Motivo |
|---|---|---|---|
| users | `ux_users_email` (único) | `email` | Login y registro: búsqueda por correo y unicidad |
| users | `_id_` (por defecto) | `_id` | Perfil por id |
| refresh_tokens | `ux_refresh_tokens_tokenHash` (único) | `tokenHash` | Refresh: búsqueda por hash del token |
| refresh_tokens | `ix_refresh_tokens_sessionId` | `sessionId` | Revocar los tokens de una sesión |
| refresh_tokens | `ttl_refresh_tokens_absoluteExpiresAtUtc` (TTL 0) | `absoluteExpiresAtUtc` | Borrado automático al vencer |
| refresh_sessions | `ix_refresh_sessions_userId` | `userId` | Sesiones de un usuario |
| refresh_sessions | `ttl_refresh_sessions_absoluteExpiresAtUtc` (TTL 0) | `absoluteExpiresAtUtc` | Borrado automático al vencer |
| pets | `ix_pets_ownerId_createdAtUtc` | `ownerId` asc, `createdAtUtc` desc | Listado del propietario, de la más reciente a la más antigua |

## Consultas críticas (CA-02): todas usan índice
Login/registro por correo, perfil por id, refresh por hash, tokens de una sesión, sesiones de un usuario, sesión por id, mascota propia por id y listado de mascotas del propietario (`ownerId` + orden `createdAtUtc`, `_id` descendente) se ejecutan con `explain` y el plan ganador contiene IXSCAN con el índice esperado y nunca COLLSCAN.

## Observación abierta
El listado ordena además por `_id` descendente como desempate (estabilidad de la paginación); el índice cubre `ownerId` y `createdAtUtc`, de modo que el desempate por `_id` puede requerir una ordenación en memoria dentro del grupo de un mismo instante. Con el máximo de 100 mascotas por propietario el costo es despreciable; si crece, añadir `_id` descendente al índice (decisión pendiente, cambia el índice).
