# Spec — pm-025 refresh-sessions-persistence

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Sesiones y tokens en Atlas con hash único, consumo atómico y TTL.

## 2. Dependencias

- [pm-012](../pm-012-atlas-connectivity/spec.md) — atlas-connectivity
- [pm-023](../pm-023-refresh-session-domain/spec.md) — refresh-session-domain

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T014; plan.md §11.2.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-025#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Solo se almacena el hash SHA-256 del refresh token, nunca el token. | T053 |
| `CA-02` | El consumo del token es atómico (un solo uso). | T053 |
| `CA-03` | Las sesiones expiradas se eliminan por TTL. | T053 |
| `CA-04` | Existe un índice único sobre el hash. | T053 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T053 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de la persistencia de sesiones |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
