# Spec — pm-023 refresh-session-domain

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Sesión con vencimiento por inactividad y absoluto y token de un solo uso.

## 2. Dependencias

- [pm-022](../pm-022-user-domain-roles/spec.md) — user-domain-roles

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T013; plan.md §11.1.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-023#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La sesión de refresh vence por inactividad (30 días) y de forma absoluta (90 días). | T052 |
| `CA-02` | El token de refresh es de un solo uso. | T052 |
| `CA-03` | `UpdateActivity` no reactiva una sesión revocada. | T052 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T052 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del dominio de sesiones |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
