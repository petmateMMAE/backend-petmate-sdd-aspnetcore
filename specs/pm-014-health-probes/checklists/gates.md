# Gates — pm-014 health-probes (T045)

> Estado (2026-11-08): T045 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-014#CA-01` | T045 | `live` devuelve 200 sin consultar dependencias | `HealthEndpointsTests.Live_returns_200_wrapper_without_checking_dependencies` | En verde |
| `pm-014#CA-02` | T045 | `ready` 200/503 con `data.dependencies` | `HealthEndpointsTests.Ready_with_healthy_dependencies_returns_200_wrapper_listing_each_one_as_healthy`, `Ready_with_unreachable_dependencies_returns_503_naming_each_unhealthy_dependency_without_secrets`; validación manual con Atlas y Redis reales | En verde |
| `pm-014#CA-03` | T045 | No expone hosts, URIs ni secretos | `Ready_with_unreachable_dependencies_returns_503_…_without_secrets`; `MongoAtlasIntegrationTests.Health_check_reports_unhealthy_without_details_when_mongo_is_unreachable` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: La readiness contra Atlas real se verificó a mano (hoy con `docker compose`: mongodb, redis-auth y redis-cache sanos).
- [ ] Revisión independiente.
- [x] Aprobación humana de T045 (2026-11-08).
