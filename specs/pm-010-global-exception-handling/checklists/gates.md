# Gates — pm-010 global-exception-handling (T043)

> Estado (2026-11-08): T043 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-010#CA-01` | T043 | 500 con wrapper y sin detalles | `ApiResponseSecurityTests.Middleware_hides_unexpected_exception_details`, `ResponseWrapperHttpTests` (500) | En verde |
| `pm-010#CA-02` | T043 | Dependencias críticas no disponibles → 503 | `ApiResponseSecurityTests`/`PersistenceSecurityTests.Repositories_translate_provider_failures_without_leaking_connection_details`; contrato `Login_fails_closed_with_503_when_the_attempt_store_is_down`, `Logout_without_refresh_token_returns_400_and_with_redis_down_returns_503` | En verde |
| `pm-010#CA-03` | T043 | La cancelación del cliente no es un 500 | El middleware captura `OperationCanceledException` con `RequestAborted` y no escribe respuesta; sin prueba automática directa | Parcial (código revisado; sin prueba directa) |
| `pm-010#CA-04` | T043 | Mensajes acotados que no reflejan valores | `ErrorBoundsSecurityTests.BoundErrors_limits_count_and_text_length`, `Middleware_maps_bad_http_request_to_client_error_without_framework_message` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: La cancelación por el cliente solo está cubierta por revisión de código; conviene una prueba directa (mejora propuesta, no bloqueante).
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T043.
