# Gates — pm-061 api-docs-contract-sync (T024, T039, T073)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-061#CA-01` | T024 | Contratos de autenticación documentados y sincronizados con el catálogo | `docs/api/endpoints.md` (§Autenticación inicial, cuatro rutas con ejemplos, 429, bloqueo); `RouteCatalogTests`; `AuthEndpointsTests`, `AuthEndpointValidationTests`, `AuthThrottlingEndpointsTests`, `SessionLifecycleTests` | Cumple |
| `pm-061#CA-02` | T039 | Contratos de mascotas documentados y probados contra pm-040 a pm-044 | `docs/api/endpoints.md` (§Mascotas, paginación, límites); `PetEndpointsTests`, `PetListEndpointsTests`, `PetLimitsTests`; `PetOwnershipSecurityTests` | Cumple |
| `pm-061#CA-03` | T073 | Rutas del código coinciden automáticamente con el catálogo | `tests/PetMate.ContractTests/Architecture/RouteCatalogTests.cs` (entregada en T055): catálogo ↔ rutas mapeadas y reglas de nombrado | Cumple |
| `pm-061#CA-04` | T073 | OpenAPI representa el Response Wrapper | `OpenApiWrapperTests.Every_response_body_in_the_openapi_document_uses_the_response_wrapper_schema` (≥ 40 respuestas, todas con esquema `ApiResponse*` y exactamente `statusCode`/`message`/`data`) y `Every_business_route_documents_the_wrapper_for_success_and_unauthorized` | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 188/188, Security 177/177, Contract 188/188, Integration 56 (34 pasan y 22 se omiten sin contraseñas reales de Redis; las 56 pasaron con los Redis desechables en la corrida previa) | Mismo contenedor, repositorio en solo lectura, sin `.env` salvo la corrida de Atlas |
| Documento OpenAPI | Se genera en Development | `/openapi/v1.json`; en Production sigue oculto (`OpenApiEndpointsTests`) |
- Limitaciones: La evidencia de T024 y T039 se reutiliza de pruebas existentes (no se reescribe el histórico); OpenAPI describe los códigos declarados con `ProducesResponseType`, no los 429 que puedan surgir del middleware global de límites.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de las Tasks de este Spec (2026-11-08).
