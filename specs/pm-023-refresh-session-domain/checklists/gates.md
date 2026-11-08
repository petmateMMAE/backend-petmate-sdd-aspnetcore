# Gates — pm-023 refresh-session-domain (T052)

> Estado (2026-11-08): T052 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-023#CA-01` | T052 | Vencimiento por inactividad (30 días) y absoluto (90 días) | `SessionTests.Default_policy_is_30_days_idle_and_90_days_absolute`, `Session_expires_after_idle_timeout`, `RegisterUse_renews_idle_window_but_never_beyond_absolute_limit`; `RestoreAndHardeningTests.Session_is_inactive_exactly_at_idle_and_absolute_limits` | En verde |
| `pm-023#CA-02` | T052 | Token de un solo uso | `SessionTests.Token_is_consumed_only_once` | En verde |
| `pm-023#CA-03` | T052 | `UpdateActivity` no reactiva una sesión revocada | `SessionTests.RegisterUse_fails_for_expired_or_revoked_session`; `AuthPersistenceIntegrationTests.Activity_updates_never_modify_an_already_revoked_session` (Atlas) | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: Sin limitaciones relevantes identificadas.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T052.
