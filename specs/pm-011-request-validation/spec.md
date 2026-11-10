# Spec — pm-011 request-validation

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Validación de solicitudes antes del handler con 400 y errores en español.

## 2. Dependencias

- [pm-006](../pm-006-cqrs-mediator/spec.md) — cqrs-mediator
- [pm-009](../pm-009-response-wrapper-contract/spec.md) — response-wrapper-contract
- [pm-010](../pm-010-global-exception-handling/spec.md) — global-exception-handling

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-011; task T009.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-011#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un request inválido devuelve 400 con wrapper y `data.errors[{field,message}]` en español, y no ejecuta el handler. [origen: 00-foundation CA-011] | T009 |
| `CA-02` | Máximo 50 errores y 200 caracteres por texto; los mensajes no incluyen el valor recibido. | T009 |
| `CA-03` | Los errores de binding y JSON de MVC usan el mismo formato. | T009 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | Validación de requests y contratos básicos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
