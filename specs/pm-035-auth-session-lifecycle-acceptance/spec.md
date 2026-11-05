# Spec — pm-035 auth-session-lifecycle-acceptance

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Escenarios de ciclo de vida de sesión entre endpoints y cierre del grupo de autenticación.

## 2. Dependencias

- [pm-028](../pm-028-token-revocation-redis/spec.md) — token-revocation-redis
- [pm-029](../pm-029-user-registration/spec.md) — user-registration
- [pm-030](../pm-030-user-login/spec.md) — user-login
- [pm-031](../pm-031-refresh-token-rotation/spec.md) — refresh-token-rotation
- [pm-032](../pm-032-user-logout/spec.md) — user-logout

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T025.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6, §10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-035#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los escenarios de ciclo de vida de sesión entre Register, Login, Refresh y Logout pasan de extremo a extremo. [origen: 01-initial-authentication CA-012] | T025 |
| `CA-02` | Los gates del grupo de autenticación tienen evidencia y revisión independiente. | T025 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T025 | Histórica | Propuesto | Gates y revisión independiente de auth |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Spec de aceptación (reemplaza a los criterios duplicados del slice anterior).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
