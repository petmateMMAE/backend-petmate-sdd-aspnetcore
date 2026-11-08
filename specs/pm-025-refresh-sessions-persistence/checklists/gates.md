# Gates — pm-025 refresh-sessions-persistence (T053)

> Estado (2026-11-08): T053 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-025#CA-01` | T053 | Solo se almacena el hash SHA-256 del token | `AuthPersistenceIntegrationTests.Session_and_token_round_trip_storing_only_the_hash`; `AuthFlowIntegrationTests` (el documento no contiene el token) | En verde |
| `pm-025#CA-02` | T053 | Consumo atómico | `AuthPersistenceIntegrationTests.Concurrent_token_consumption_succeeds_for_exactly_one_caller`; `AuthFlowIntegrationTests.Concurrent_refresh_with_the_same_token_lets_exactly_one_request_win_…` | En verde |
| `pm-025#CA-03` | T053 | Expiración por TTL | `AuthPersistenceIntegrationTests.Indexes_exist_with_expected_uniqueness_and_ttl_and_creation_is_idempotent` | En verde |
| `pm-025#CA-04` | T053 | Índice único sobre el hash | `AuthPersistenceIntegrationTests.Duplicate_token_hash_is_rejected_by_the_unique_index` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: Las pruebas de integración requieren Atlas con base de pruebas (hoy se ejecutaron contra Atlas real). El borrado real por el TTL lo hace el servidor en segundo plano y no se esperó en las pruebas.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T053.
