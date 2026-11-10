# Gates — pm-031-refresh-token-rotation

## Enmienda 2026-11-08 — T099 (H8)

> Estado: implementada y verificada; pendiente de revisión independiente y de aprobación humana. Aprobada por el usuario (2026-11-08).

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-031#CA-04` | T099 | Ventana de gracia de 10 s: reutilizar un token consumido no revoca; después sí | `RefreshHandler.ReplayGrace`; `AuthHandlersTests.Presenting_a_consumed_token_inside_the_grace_window...`, `Replay_after_the_grace_window...`, `Concurrent_refresh_requests_with_the_same_token_let_exactly_one_succeed` (ahora la sesión NO se revoca); `SessionLifecycleTests`, `AuthEndpointsTests.Refresh_rotates...` | Cumple |

| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 215, Security 180, Contract 202 (medidos al terminar esta Task; ver Memoria.md), Integration con los Redis desechables: 24 pasan y 4 se omiten del filtro Redis | Mismo contenedor; Redis con dos contenedores desechables de contraseñas aleatorias |
| Integridad SDD | OK | `scripts/check-sdd.ps1` (rango ampliado a T001–T102) |

- Limitaciones: Un atacante que reutilice un token robado dentro de los 10 s no activa la revocación, pero tampoco recibe tokens (401). El cliente que perdió la respuesta no puede recuperarla: debe iniciar sesión de nuevo (opción B queda para si se necesita).
- [ ] Revisión independiente.
- [x] Aprobación humana de T099 (2026-11-08).
