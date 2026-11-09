# Spec — pm-059 release-versioning

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Versionado SemVer, etiquetas y versión trazable a la imagen.

## 2. Dependencias

- [pm-055](../pm-055-ci-pull-request-pipeline/spec.md) — ci-pull-request-pipeline

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-09 (ver [traceability.md](../traceability.md)).
- Constitución: §8.13.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-059#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las versiones siguen SemVer. | T071 |
| `CA-02` | Cada release tiene una etiqueta de Git, creada solo con autorización expresa. | T071 |
| `CA-03` | La versión es trazable a la imagen de contenedor. | T071 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T071 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; primera ejecución real en GitHub pendiente) | Definir el versionado de releases |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
