# Spec — pm-005 dependency-injection-composition

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Módulos de DI por capa sin Service Locator.

## 2. Dependencias

- [pm-002](../pm-002-clean-architecture-dependency-rules/spec.md) — clean-architecture-dependency-rules

## 3. Origen

- Fuentes: 00-foundation/task.md T005.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §2, §3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-005#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El registro de dependencias se hace por capa (`AddApplication`, `AddInfrastructure`) desde el composition root de Api. | T005 |
| `CA-02` | No se usa Service Locator. | T005 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T005 | Histórica | Cerrado (2026-11-03) | Módulos de inyección de dependencias |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
