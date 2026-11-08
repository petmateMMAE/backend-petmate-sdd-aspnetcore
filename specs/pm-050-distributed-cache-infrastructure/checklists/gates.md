# Gates — pm-050 distributed-cache-infrastructure (T061)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-050#CA-01` | T061 | Usa exclusivamente Redis Cache | `RedisCacheStore` depende solo de `RedisCacheConnection`; `CachePolicyTests.The_cache_adapter_depends_only_on_the_cache_connection_and_never_on_redis_auth` | Cumple |
| `pm-050#CA-02` | T061 | Convención de claves, TTL y política de falla | `CacheKeyTests` (18 casos), `CacheTtl`; documentado en `ICacheStore` y `docs/security/cache-policy.md` | Cumple |
| `pm-050#CA-03` | T061 | Una falla de caché no afecta la corrección | `RedisCacheStoreTests.When_redis_cache_is_unreachable_every_operation_degrades_to_a_miss_without_throwing_and_quickly`; la cancelación del llamador sí se propaga | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit, Security, Contract e Integration en verde (ver Memoria.md para los conteos de esta fecha) | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Nada consume la caché todavía (pm-051); la prueba con Redis real (`Set_get_and_remove_use_the_convention_with_a_ttl`) se omite sin `PETMATE_IT_REDIS_CACHE` real.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T061.
