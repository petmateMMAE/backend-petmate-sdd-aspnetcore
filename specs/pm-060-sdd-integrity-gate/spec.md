# Spec — pm-060 sdd-integrity-gate

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Validación automática de la integridad de specs, tasks y trazabilidad.

## 2. Dependencias

- [pm-055](../pm-055-ci-pull-request-pipeline/spec.md) — ci-pull-request-pipeline

## 3. Origen

- Fuentes: nuevo; script inicial `scripts/check-sdd.ps1` entregado en la migración.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §10, §12.3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-060#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Una validación automática comprueba IDs consecutivos, dependencias válidas sin ciclos, una Task con un solo Spec principal y CA sin duplicados. | T072 |
| `CA-02` | La validación comprueba la trazabilidad y se ejecuta en CI. | T072 |
| `CA-03` | Ante cualquier inconsistencia la validación falla. | T072 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T072 | Nueva | En progreso | Integrar el gate de integridad SDD en CI |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El script inicial no está integrado en CI; T072 sigue Propuesta.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
