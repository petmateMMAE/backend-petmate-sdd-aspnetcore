# Spec — pm-022 user-domain-roles

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L1 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Usuario, correo normalizado y roles Owner/Business sin Admin.

## 2. Dependencias

- [pm-001](../pm-001-solution-structure/spec.md) — solution-structure

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-010; task T013.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-022#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El usuario tiene correo normalizado único y rol Owner o Business; no existe rol Admin. [origen: 01-initial-authentication CA-010] | T013 |
| `CA-02` | El cliente no puede autoasignarse el rol Business. | T013 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T013 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Modelar usuario, roles y sesión |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
