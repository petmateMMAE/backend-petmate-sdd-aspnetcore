# Gates — pm-035 auth-session-lifecycle-acceptance (T025)

> Estado: T025 cerrada con aprobación humana el 2026-11-08, tras aprobar pm-028 a pm-032 y cerrar T023 (pm-034). pm-035 `Cerrado`.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-035#CA-01` | T025 | `SessionLifecycleTests.Sessions_of_one_user_are_independent_across_register_login_refresh_replay_and_logout` y `Two_users_never_share_sessions_even_with_the_same_flow` (HTTP); `AuthFlowIntegrationTests`: `Register_login_refresh_and_logout_work_end_to_end_with_real_atlas_and_redis`, `Replay_of_a_rotated_token_revokes_the_whole_family_in_atlas`, `Concurrent_refresh_with_the_same_token_lets_exactly_one_request_win_…`, `Sessions_expire_after_the_idle_window_and_users_cannot_log_out_each_other`, `Logout_with_redis_auth_down_fails_closed_…`, `Login_lockout_works_end_to_end_…` | En verde |
| `pm-035#CA-02` | T025 | Evidencia de los gates reunida abajo y revisión independiente de seguridad y de trazabilidad del grupo de autenticación | Realizada (2026-11-08); ver hallazgos |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 170/170, Integration 56/56 | Mismo contenedor |
| Auditoría NuGet | vulnerabilidades (desde High), desuso y licencias en verde | `scripts/quality-gate.ps1 -AuditOnly` (2026-11-08) |
| Kestrel y Redis reales | Lockout: 5 fallos → 401, el sexto → 429 con `Retry-After: 900` | `Invoke-WebRequest` local |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

## Revisión independiente del grupo de autenticación (2026-11-08)
- **Seguridad:** APTO CON OBSERVACIONES (sin hallazgos críticos ni altos). Comprobado y correcto: refresh tokens opacos de 256 bits guardados solo como hash, rotación atómica y detección de replay, Argon2id con parámetros validados, login anti-enumeración con hash ficticio, JWT HS256 fijo con validación completa y falla cerrada, logout de sesiones ajenas (mismo 403), política de denegación por defecto, mass assignment bloqueado y errores genéricos. Hallazgos y estado en [pm-034 gates](../../pm-034-auth-brute-force-protection/checklists/gates.md): H1, H2 y H5 corregidos (más una carrera del script de Redis hallada al probar H1); H3, H4, H6, H7, H8 e I2 abiertos para decisión.
- **Trazabilidad:** CONSISTENTE CON OBSERVACIONES (sin discrepancias de código). Corregidas: ASVS checklist (D1), estados de los endpoints de autenticación (D2), objetivo de pm-036 y registro (D3, D4), matriz de límites con el 503 del hasher (D7). Pendientes por naturaleza: D6 (dependencias formales de pm-035) y las casillas de Definition of Done de las Tasks cerradas sin revisión independiente propia.

- [x] Aprobación de los specs pm-028 a pm-032 (2026-11-08).
- [x] Cierre de T023 (pm-034, 2026-11-08).
- [x] Aprobación humana del cierre de T025 (2026-11-08).
