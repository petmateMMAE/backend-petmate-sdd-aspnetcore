# Spec — pm-058 sbom-generation

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Generación de SBOM por compilación.

## 2. Dependencias

- [pm-055](../pm-055-ci-pull-request-pipeline/spec.md) — ci-pull-request-pipeline

## 3. Origen

- Fuentes: nuevo (constitución §9).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-058#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cada compilación genera un SBOM con una herramienta de costo $0. | T070 |
| `CA-02` | El SBOM se conserva como artefacto del pipeline. | T070 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T070 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Generar el SBOM por compilación |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
