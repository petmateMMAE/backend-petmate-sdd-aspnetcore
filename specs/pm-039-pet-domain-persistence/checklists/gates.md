# Gates — pm-039 pet-domain-persistence

> Estado: T029 cerrada con aprobación humana el 2026-11-08 (sin revisión independiente). T036 y T037 cerradas con aprobación humana el 2026-11-08 (sin revisión independiente). pm-039 queda Cerrado.

## Trazabilidad
| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-039#CA-01` | T029 | `PetTests` (creación, recorte, longitudes en el límite, fecha futura y de más de 50 años, sexo inválido, foto no HTTPS o con credenciales, actualización, restauración, `PetValidation`) | En verde |
| `pm-039#CA-02` | T029 | `PetPersistenceIntegrationTests.Owner_index_exists_and_creation_is_idempotent` (Atlas) | En verde |
| `pm-039#CA-03` | T029 | `Delete_removes_the_document_physically`, `Another_owner_cannot_read_update_or_delete_the_pet` (Atlas) | En verde |
| `pm-039#CA-04` | T036 | `PetTests` (19: dominio y `PetValidation`) y `PetHandlersTests` (5: handlers y validadores con reloj del servidor) | En verde (142/142 unitarias); T036 cerrada |
| `pm-039#CA-05` | T037 | `PetPersistenceIntegrationTests` (8 al cierre de T037; 9 desde que pm-041 añadió `Count_by_owner_counts_only_the_owner_pets_and_follows_deletes`): las 6 de T029 más `Owner_listing_query_is_served_by_the_owner_index` (el plan ganador es IXSCAN sobre `ix_pets_ownerId_createdAtUtc`, sin COLLSCAN) y `Concurrent_deletes_of_the_same_pet_let_exactly_one_succeed_and_later_updates_find_nothing` | En verde contra Atlas; T037 cerrada |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura y sin `.env` |
| Pruebas unitarias | 137/137 (19 nuevas) | Mismo contenedor |
| Pruebas de seguridad | 155/155 | Mismo contenedor |
| Pruebas de contrato | 116/116 | Mismo contenedor |
| Integración contra Atlas de pruebas | Las 6 nuevas de mascotas y las de persistencia de autenticación en verde; Redis omitidas (sin `PETMATE_IT_REDIS_*`) | Contenedor con `--env-file .env` (sin imprimir valores); Atlas respondió al ping |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

## Pendiente
- [ ] Revisión independiente de seguridad y trazabilidad.
- [x] Aprobación humana del cierre de T029 (2026-11-08).

## Evidencia T036 y T037 (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, copia sin `.env`, `--cap-drop ALL` |
| Pruebas unitarias | 142/142 | Mismo contenedor |
| Integración contra Atlas de pruebas | 32 aprobadas y 15 omitidas (Redis y flujos que lo requieren, sin `PETMATE_IT_REDIS_*`); las 8 de mascotas en verde | Mismo contenedor con `--env-file .env` (sin leer ni imprimir valores) |
| Paquetes nuevos | Ninguno | — |

- Observación (no bloqueante): `ListByOwner` ordena además por `_id`, que no está en el índice; Atlas usa el índice para filtrar y ordena en memoria el desempate. Con el volumen por propietario esperado no es un problema; se revisará en pm-052.
- [x] Aprobación humana del cierre de T036 y T037 (2026-11-08).
