# Gates — pm-054 structured-logging-pii-safe (T065)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-054#CA-01` | T065 | Logs estructurados con identificador de correlación | `RequestLoggingMiddleware`; `ApplicationBootstrap` (`AddJsonConsole`, `IncludeScopes`); `RequestLoggingTests.Console_logs_are_json_with_scopes`, `Every_response_carries_a_correlation_id_and_a_safe_supplied_one_is_kept`, `An_unsafe_supplied_correlation_id_is_replaced` (3 casos), `The_request_log_is_one_event_per_request_with_route_template_status_and_correlation_scope` | Cumple |
| `pm-054#CA-02` | T065 | Sin credenciales, tokens ni PII en los logs | Diseño: solo método, plantilla de ruta, estado y duración; filtro `Hosting.Diagnostics = None` (evita `RequestPath`); `The_request_log_...` verifica que ni el id real de la ruta ni la consulta aparecen | Cumple |
| `pm-054#CA-03` | T065 | Pruebas que verifican la ausencia de secretos | `RequestLoggingTests.Logs_never_contain_credentials_tokens_emails_names_or_request_bodies`: captura mensaje, estado, ámbitos y excepción de todo el flujo (registro, login válido e inválido, creación de mascota, perfil, token falso, JSON roto, cuerpo de 20 000 caracteres, refresh y replay) y busca correo, contraseñas, access y refresh tokens, nombre de mascota, firma falsa, clave JWT y `RequestPath` | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 188/188, Security 177/177, Contract 188/188, Integration 56 (34 pasan y 22 se omiten sin contraseñas reales de Redis; las 56 pasaron con los Redis desechables en la corrida previa) | Mismo contenedor, repositorio en solo lectura, sin `.env` salvo la corrida de Atlas |
| Hallazgo durante la implementación | Corregido | La primera versión filtraba el id real y el texto de la consulta mediante el ámbito `RequestPath` de Hosting; la prueba lo detectó y se desactivó ese ámbito |
| Escáner de secretos del repositorio | Pasa | Un JWT literal de la prueba fue detectado por `SecretScanTests`; se construye en tiempo de ejecución |
- Limitaciones: No se registran eventos de seguridad (I2); el correlativo no se propaga a otros servicios; se probó con el logger de captura y la configuración del formateador, no se capturó la salida real de consola en Azure.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de las Tasks de este Spec.
