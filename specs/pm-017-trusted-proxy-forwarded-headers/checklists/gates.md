# Gates — pm-017 trusted-proxy-forwarded-headers (T047, T097)

> Estado (2026-11-08): retrospectiva y Task nueva aprobadas por el usuario (2026-11-08); sin revisión independiente de seguridad de la Task nueva (dispensa del usuario registrada en `specs/README.md`).

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-017#CA-01` | T047 | Con `TRUSTED_PROXIES`, la IP sale de `X-Forwarded-For` solo de los proxies listados | `RateLimitAndProxyTests.Valid_trusted_proxies_are_accepted`; `ForwardedHeadersSpoofingTests.Control_the_header_is_honored_only_when_the_connection_comes_from_a_listed_proxy` | En verde |
| `pm-017#CA-02` | T047 | Sin `TRUSTED_PROXIES`, `X-Forwarded-*` se ignora | `ForwardedHeadersSpoofingTests.Without_trusted_proxies_a_forged_x_forwarded_for_does_not_evade_the_ip_limit`, `Forged_forwarded_proto_and_host_are_ignored_and_never_echoed` | En verde |
| `pm-017#CA-03` | T047 | Valor inválido impide el arranque sin mostrarlo | `RateLimitAndProxyTests.Invalid_proxy_or_rate_limit_settings_fail_fast_without_echoing_the_value` | En verde |
| `pm-017#CA-04` | T097 | Un cliente no confiable no falsifica su IP | `ForwardedHeadersSpoofingTests.With_a_trusted_proxy_list_that_does_not_include_the_client_forged_headers_are_still_ignored` y `Without_trusted_proxies_…` (8 solicitudes con IP distintas en la cabecera: 3 pasan y 5 reciben 429); control positivo con el proxy listado | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: En el servidor de pruebas sin IP de origen la comprobación no es fiel, por eso las pruebas fijan una; Kestrel siempre trae la IP de la conexión. Detrás del balanceador de Azure `TRUSTED_PROXIES` debe listar el último salto (requisito de despliegue, H6).
- [ ] Revisión independiente.
- [x] Aprobación humana (2026-11-08) del cierre de T047, T097.
