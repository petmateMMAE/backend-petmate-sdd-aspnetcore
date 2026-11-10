# Plan — pm-035 auth-session-lifecycle-acceptance (T025)

## Enfoque
- **CA-01:** escenarios de ciclo de vida entre Register, Login, Refresh y Logout con el JWT real: `SessionLifecycleTests` (HTTP, dobles en memoria) y `AuthFlowIntegrationTests` (Atlas y Redis reales). Se añadieron los dos escenarios multisesión que faltaban.
- **CA-02:** gates del grupo de autenticación con evidencia reproducible y revisión independiente (seguridad y trazabilidad, 2026-11-08, dos agentes de solo lectura), con los hallazgos corregidos o documentados.
- **Dependencias:** pm-035 declara pm-028 a pm-034. pm-033 está cerrado; pm-034 espera T023; pm-028 a pm-032 siguen «En revisión» (sus Tasks T017 a T021 están cerradas). Esos specs se aprobaron el 2026-11-08, por lo que T025 pudo cerrarse.
