# Gates — pm-013 redis-dual-connectivity (T044)

> Estado (2026-11-08): T044 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-013#CA-01` | T044 | Dos conexiones independientes con configuración distinta | `RedisIntegrationTests.Auth_and_cache_are_independent_instances`, `Memory_policies_match_responsibilities` (Redis real); `TokenRevocationStoreTests.Store_depends_only_on_the_auth_redis_connection_never_on_the_cache` | En verde |
| `pm-013#CA-02` | T044 | Fuera de Development exige TLS | `InfrastructureSettingsTests` (configuración sin `ssl=True` rechazada fuera de Development) | En verde |
| `pm-013#CA-03` | T044 | Conexión perezosa y reconectable | `RedisIntegrationTests.Health_check_reports_unhealthy_without_details_when_redis_is_unreachable`, `TokenRevocationStoreTests.Redis_outage_fails_closed_for_both_reads_and_writes` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: Las pruebas de integración requieren Redis y se omiten sin él (hoy se ejecutaron con Redis temporales). El TLS contra los Redis de Azure no está cubierto aquí (pm-070 y pm-071).
- [ ] Revisión independiente.
- [x] Aprobación humana de T044 (2026-11-08).
