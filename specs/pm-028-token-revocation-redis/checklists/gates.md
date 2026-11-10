# Gates — pm-028-token-revocation-redis

## Enmienda 2026-11-08 — T098 (H7)

> Estado: implementada y verificada; pendiente de revisión independiente y de aprobación humana. Aprobada por el usuario (2026-11-08).

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-028#CA-04` | T098 | Access token ligado a la sesión (`sid`) y rechazado al revocarla, sin esperar a expirar | `JwtAccessTokenService` (claim `sid`, `Lifetime`), `RedisTokenRevocationStore.RevokeSession/IsSessionRevoked`, `RevocationJwtBearerEvents`; `AuthHardeningTests.An_access_token_bound_to_a_session_stops_working...`; `SessionLifecycleTests` (a.Access y a2.Access dan 401 tras el replay); `AuthHandlersTests.Replay_after_the_grace_window...` y `Logout_revokes_the_session...`; `TokenRevocationStoreTests.A_revoked_session_is_reported_revoked_with_a_ttl...` (Redis real) | Cumple |

| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 215, Security 180, Contract 202 (medidos al terminar esta Task; ver Memoria.md), Integration con los Redis desechables: 24 pasan y 4 se omiten del filtro Redis | Mismo contenedor; Redis con dos contenedores desechables de contraseñas aleatorias |
| Integridad SDD | OK | `scripts/check-sdd.ps1` (rango ampliado a T001–T102) |

- Limitaciones: Los access tokens emitidos antes del cambio no tienen `sid` y valen hasta expirar (15 minutos como máximo). Cuesta una lectura de Redis más por solicitud autenticada. Si Redis falla, la solicitud falla cerrada (503).
- [ ] Revisión independiente.
- [x] Aprobación humana de T098 (2026-11-08).
