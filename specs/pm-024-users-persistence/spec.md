# Spec — pm-024 users-persistence

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Persistencia de usuarios en Atlas con correo único y arranque de índices.

## 2. Dependencias

- [pm-012](../pm-012-atlas-connectivity/spec.md) — atlas-connectivity
- [pm-022](../pm-022-user-domain-roles/spec.md) — user-domain-roles

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T014; plan.md §11.2.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §4, §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-024#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los usuarios se persisten en Atlas con índice único sobre el correo normalizado. | T014 |
| `CA-02` | Los índices se inicializan al arranque. | T014 |
| `CA-03` | La persistencia está verificada contra la base de pruebas de Atlas. | T014 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Persistencia Atlas de usuarios y sesiones |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
