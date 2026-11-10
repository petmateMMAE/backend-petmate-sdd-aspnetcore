# Spec — pm-009 response-wrapper-contract

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Contrato `statusCode/message/data` con `data` nulo y coincidencia con HTTP.

## 2. Dependencias

- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-004; task T008.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-009#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | `ApiResponse` expone `statusCode`, `message` y `data`, y `data: null` es serializable. [origen: 00-foundation CA-004] | T008 |
| `CA-02` | El `statusCode` del wrapper coincide con el código HTTP. | T008 |
| `CA-03` | Las respuestas 401, 403, 404, 405 y demás 4xx/5xx sin cuerpo se envuelven con `data: null`. | T008 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Response Wrapper y middleware de errores |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
