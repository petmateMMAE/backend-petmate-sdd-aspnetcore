# Gates — pm-036 rate-limiting (T054 y T057)

> Estado: T054 (retrospectiva, aprobada) y T057 (cerrada) con aprobación humana el 2026-11-08 (sin revisión independiente de las Tasks); pm-036 `Verificado`. La retrospectiva T054 queda «Aprobada y documentada» y no «Cerrado», porque el gate SDD (`check-sdd.ps1`) prohíbe cerrar retrospectivas; cambiar esa regla exige tu autorización.

| CA | Task | Pruebas / evidencia | Resultado |
|---|---|---|---|
| `pm-036#CA-01` | T054 | `AuthEndpointsTests.Login_is_throttled_with_429_wrapper_and_retry_after_after_the_configured_limit`; `RateLimitAndProxyTests.Invalid_proxy_or_rate_limit_settings_fail_fast_without_echoing_the_value` | En verde |
| `pm-036#CA-02` | T054 | `RateLimitAndProxyTests`: `IPv4_clients_are_partitioned_by_full_address_and_missing_ip_shares_one_partition`, `IPv6_addresses_in_the_same_slash_64_share_a_partition_and_other_prefixes_do_not`, `IPv4_mapped_IPv6_addresses_use_the_IPv4_key`, `Valid_trusted_proxies_are_accepted` | En verde |
| `pm-036#CA-03` | T057 | Matriz en `docs/api/endpoints.md`; `AuthThrottlingEndpointsTests.Profile_endpoints_are_throttled_per_user_with_429_wrapper_and_retry_after`; `PetLimitsTests.Pets_endpoints_are_throttled_per_user_…`; `RateLimitAndProxyTests` (valores inválidos de `PETS_` y `USERS_RATE_LIMIT_PER_MINUTE` fallan al arrancar sin mostrar el valor) | En verde |
| `pm-036#CA-04` | T054 | Contrato: 429 con el wrapper y `Retry-After` en login, mascotas y perfil | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 186/186, Security 169/169, Contract 168/168, Integration 53/53 | Mismo contenedor |
| Paquetes nuevos | Ninguno | — |

- Limitaciones de T054: cubre solo el límite por IP de autenticación; la revisión independiente de T018 a T020 consta solo en documentos históricos.
- [ ] Revisión independiente (no realizada para estas Tasks; el usuario aprobó el cierre sin ella).
- [x] Aprobación humana del cierre de T054 y T057 (2026-11-08).

## Enmienda 2026-11-08 — T100 (H6)

> Estado: implementada y verificada; pendiente de revisión independiente y de aprobación humana. Aprobada por el usuario (2026-11-08).

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-036#CA-05` | T100 | Límite por IP compartido entre réplicas (Redis Auth) y falla cerrada | `AuthIpThrottleFilter` (+ `AuthIpThrottleOptions`) aplicado a register, login y refresh; `AuthHardeningTests.The_ip_limit_is_shared_across_replicas_through_redis_auth` y `Authentication_routes_fail_closed_with_503...`; `AuthAttemptThrottleTests.The_hyphenated_scopes...` (Redis real) | Cumple |
| `pm-036#CA-06` | T100 | Advertencia/fallo al arrancar sin `TRUSTED_PROXIES` y saltos documentados | `StartupDiagnostics.MissingTrustedProxies`, `REQUIRE_TRUSTED_PROXIES` en `ApplicationBootstrap`; `AuthHardeningTests.Production_can_require_trusted_proxies...`; documentación en `docs/api/endpoints.md`, `docs/infrastructure/docker-redis-atlas.md` y `.env.example` | Cumple |

| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 215, Security 180, Contract 202 (medidos al terminar esta Task; ver Memoria.md), Integration con los Redis desechables: 24 pasan y 4 se omiten del filtro Redis | Mismo contenedor; Redis con dos contenedores desechables de contraseñas aleatorias |
| Integridad SDD | OK | `scripts/check-sdd.ps1` (rango ampliado a T001–T102) |

- Limitaciones: El límite fijo en Redis es «10 por minuto y espera de 1 minuto al excederlo», algo más estricto que la ventana fija en proceso. Con muchas IPs detrás de un mismo proxy sin `TRUSTED_PROXIES` el límite sigue agrupándose (por eso la advertencia). El primer despliegue deja `TRUSTED_PROXIES` vacío a propósito, por eso no es fatal por defecto.
- [ ] Revisión independiente.
- [x] Aprobación humana de T100 (2026-11-08).
