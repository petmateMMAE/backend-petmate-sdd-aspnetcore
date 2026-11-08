# Gates — pm-039 pet-domain-persistence

> Estado: T029 cerrada con aprobación humana el 2026-11-08 (sin revisión independiente). El Spec sigue en progreso hasta cerrar T036 y T037.

## Trazabilidad
| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-039#CA-01` | T029 | `PetTests` (creación, recorte, longitudes en el límite, fecha futura y de más de 50 años, sexo inválido, foto no HTTPS o con credenciales, actualización, restauración, `PetValidation`) | En verde |
| `pm-039#CA-02` | T029 | `PetPersistenceIntegrationTests.Owner_index_exists_and_creation_is_idempotent` (Atlas) | En verde |
| `pm-039#CA-03` | T029 | `Delete_removes_the_document_physically`, `Another_owner_cannot_read_update_or_delete_the_pet` (Atlas) | En verde |
| `pm-039#CA-04` | T036 | `PetTests` (evidencia aportada por T029) | En verde; la Task T036 sigue abierta |
| `pm-039#CA-05` | T037 | `PetPersistenceIntegrationTests` (6 pruebas, evidencia aportada por T029) | En verde; la Task T037 sigue abierta |

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
