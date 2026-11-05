# Spec — pm-006 cqrs-mediator

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Commands y Queries despachados por un Mediator propio con registro explícito.

## 2. Dependencias

- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-003; decisions/ADR-001; task T006.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-006#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un controlador envía un request a un caso de uso mediante Mediator sin acoplarse a handlers concretos. [origen: 00-foundation CA-003] | T006 |
| `CA-02` | El registro de handlers es explícito y existe cobertura de despacho (`MediatorDispatchTests`). | T006 |
| `CA-03` | El Mediator propio queda justificado en ADR-001 (costo $0, sin dependencia externa). | T006 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T006 | Histórica | Cerrado (2026-11-03) | CQRS y Mediator |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
