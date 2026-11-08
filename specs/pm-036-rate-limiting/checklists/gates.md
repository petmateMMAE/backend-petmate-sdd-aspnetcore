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
