# Spec — pm-010 global-exception-handling

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Manejo central de excepciones sin fuga de información, 500/503 y cancelación.

## 2. Dependencias

- [pm-009](../pm-009-response-wrapper-contract/spec.md) — response-wrapper-contract

## 3. Origen

- Fuentes: 00-foundation/task.md T008.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-010#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las excepciones no controladas devuelven 500 con wrapper y sin detalles internos. | T043 |
| `CA-02` | Las dependencias críticas no disponibles devuelven 503. | T043 |
| `CA-03` | La cancelación por el cliente no se reporta como error 500. | T043 |
| `CA-04` | Los mensajes de error están acotados y no reflejan valores recibidos. | T043 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T043 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del manejo global de excepciones |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
