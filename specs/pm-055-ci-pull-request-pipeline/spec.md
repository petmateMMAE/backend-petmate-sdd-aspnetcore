# Spec — pm-055 ci-pull-request-pipeline

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Workflow de pull request con paridad respecto al gate local y ramas protegidas.

## 2. Dependencias

- [pm-021](../pm-021-local-quality-gate/spec.md) — local-quality-gate

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-009; task T012.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §8.13, §9, §10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-055#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un workflow de pull request ejecuta restore, build, suites y escaneos disponibles sin pasos de pago. [origen: 00-foundation CA-009] | T066 |
| `CA-02` | El workflow tiene paridad de criterios con el gate local (`scripts/quality-gate.ps1`). | T066 |
| `CA-03` | Las ramas protegidas exigen los checks del workflow. | T067 |
| `CA-04` | Los escáneres no pueden omitirse ni ignorar hallazgos sin aprobación. | T067 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T012 | Histórica | Implementada; NO cerrada (2026-11-03) | CI base DevSecOps |
| T066 | Nueva | En progreso | Crear el workflow de CI de pull request |
| T067 | Nueva | Propuesto | Configurar ramas protegidas y checks requeridos |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- T012 (histórica) está implementada con controles locales verificados y NO cerrada: el workflow YAML, el escaneo de imagen y el SBOM siguen pendientes. T066 cubre CA-01 y CA-02; T067 cubre CA-03 y CA-04.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
