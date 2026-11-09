# Gates — pm-007 api-route-conventions (T042, T055)

> Estado (2026-11-08): retrospectiva y Task nueva aprobadas por el usuario (2026-11-08); sin revisión independiente de seguridad de la Task nueva (dispensa del usuario registrada en `specs/README.md`).

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-007#CA-01` | T042 | Base `/api/v1`; la alternativa no versionada no se mapea | `RouteCatalogTests.Every_business_route_uses_the_versioned_base_and_a_literal_action`; `PublicSurfaceTests` (lista exacta de rutas); `EndpointTraitTests.Every_production_controller_action_maps_to_an_endpoint_of_the_pipeline_matrix`; contrato `Only_the_versioned_technical_route_is_mapped` | En verde |
| `pm-007#CA-02` | T042 | Acción literal en la ruta | `RouteCatalogTests.Route_segments_follow_the_documented_naming_rules` (la acción es un segmento literal; solo `{id}` al final) | En verde |
| `pm-007#CA-03` | T042 | `mediator-probe` identificada como no funcional | `RouteCatalogTests.The_technical_probe_is_identified_as_non_business_in_the_catalog`; fila del catálogo «no funcional de negocio» | En verde |
| `pm-007#CA-04` | T055 | Verificación automática rutas ↔ catálogo | `RouteCatalogTests.Catalog_rows_and_production_routes_are_the_same_set` (conjuntos idénticos; falla si falta una fila o sobra una ruta) | En verde |
| `pm-007#CA-05` | T055 | Reglas de nombrado documentadas y verificadas | `RouteCatalogTests.Route_segments_follow_the_documented_naming_rules` y `The_naming_rules_are_documented_in_the_catalog`; sección «Reglas de nombrado de rutas» de `docs/api/endpoints.md` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: La comprobación del catálogo toma solo las filas de una ruta por línea con el patrón `| MÉTODO | \`/api/v1/...\` |`; las filas con varias rutas y las de OpenAPI/Scalar (solo Development) no entran. Un subrecurso nuevo distinto de `me` exige cambiar la regla y la prueba.
- [ ] Revisión independiente.
- [x] Aprobación humana (2026-11-08) del cierre de T042, T055.
