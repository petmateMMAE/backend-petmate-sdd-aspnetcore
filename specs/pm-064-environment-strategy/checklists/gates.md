# Gates — pm-064 environment-strategy (T076)

> Estado (2026-11-08): T076 documentada y verificada; pendiente de revisión y aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-064#CA-01` | T076 | Entornos (desarrollo, pruebas, demo) y diferencias definidos | `docs/infrastructure/environments.md` §1; `EnvironmentStrategyTests.The_environment_document_describes_the_three_environments_and_the_conventions` | Cumple |
| `pm-064#CA-02` | T076 | Cada entorno usa una base de Atlas distinta | `EnvironmentStrategyTests.Development_test_and_demo_use_three_different_atlas_databases` (lee `.env.example` e `infra/azure/variables.tf`); `AuthPersistenceIntegrationTests.Guard_rejects_unsafe_test_database_names` (el guard de pruebas) | Cumple |
| `pm-064#CA-03` | T076 | Convenciones de nombres y etiquetas de recursos Azure | `environments.md` §3; `EnvironmentStrategyTests.Azure_resources_follow_the_documented_naming_patterns` y `Cost_tags_are_defined_and_applied_to_the_resources_of_the_app` | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` |
| Suites | Unit 188/188, Security 177/177, Contract 179/179, Integration 56/56 | Mismo contenedor (SecurityTests ejecutadas tras añadir las pruebas de esta Task) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Recursos de Azure creados | Ninguno | No se ejecutó `terraform` ni `az` |

- Limitaciones: Los valores de `.env.example` son marcadores (`…_ficticio`); el entorno Azure se llama «prod» en recursos y etiquetas pero funciona como demo; no existe un cuarto entorno.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de T076 (2026-11-08).
