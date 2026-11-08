# Spec — pm-003 code-standards-central-packages

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L1 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Analizadores, advertencias como error y versiones centralizadas de paquetes.

## 2. Dependencias

- [pm-001](../pm-001-solution-structure/spec.md) — solution-structure

## 3. Origen

- Fuentes: 00-foundation/task.md T003; plan.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §3, §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-003#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los analizadores de código y las advertencias como error están activos en todos los proyectos. | T003 |
| `CA-02` | Las versiones de paquetes se administran de forma central (`Directory.Packages.props`). | T003 |
| `CA-03` | Solo se incorporan dependencias con licencia revisada y costo $0. | T003 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T003 | Histórica | Cerrado (2026-11-03) | Estándares y administración de dependencias |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
