# Gates — pm-040 pet-create (T030)

> Estado: T030 cerrada con aprobación humana el 2026-11-08 (sin revisión independiente de la Task). El módulo de mascotas tuvo una revisión independiente de seguridad y trazabilidad el 2026-11-08 (ver [pm-045 gates](../../pm-045-pet-ownership-bola/checklists/gates.md)).

| CA | Task | Pruebas (endpoint `pets-create`) | Resultado |
|---|---|---|---|
| `pm-040#CA-01` | T030 | `tests/PetMate.ContractTests/Pets/PetEndpointsTests.cs`: `Create_returns_201_wrapper_with_the_pet_and_never_exposes_the_owner`; `PetHandlersTests.Create_assigns_the_owner_from_the_command_and_persists`; Atlas: `Pet_round_trips_with_all_fields_and_utc_dates` | En verde |
| `pm-040#CA-02` | T030 | `Create_returns_201_…` (el propietario guardado es el del token), `Create_rejects_owner_id_or_server_fields_in_the_body_and_creates_nothing` (`ownerId`, `id`, `createdAt` → 400), `PetHandlersTests.Create_assigns_the_owner_from_the_command_and_persists` | En verde |
| `pm-040#CA-03` | T030 | `Create_returns_201_wrapper_…` (statusCode 201 y claves del wrapper) | En verde |
| `pm-040#CA-04` | T030 | `Create_with_an_invalid_field_returns_400_naming_the_field` (6 casos), `Create_with_an_empty_or_malformed_body_returns_400_wrapper`, `Create_requires_an_owner_token` (401 y 403 Business, sin crear) | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y Redis temporales |
| Suites (ejecución más reciente) | Unit 172/172, Security 162/162, Contract 157/157, Integration 49/49 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- [ ] Revisión independiente específica de T030 (cubierta por la del módulo, pm-045 T040).
- [x] Aprobación humana del cierre de T030 (2026-11-08).
