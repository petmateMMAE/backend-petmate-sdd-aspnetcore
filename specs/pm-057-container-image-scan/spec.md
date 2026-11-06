# Spec — pm-057 container-image-scan

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Escaneo de la imagen de contenedor con herramienta $0.

## 2. Dependencias

- [pm-015](../pm-015-api-container-image/spec.md) — api-container-image
- [pm-055](../pm-055-ci-pull-request-pipeline/spec.md) — ci-pull-request-pipeline

## 3. Origen

- Fuentes: nuevo (constitución §9).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §9, §8.13.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-057#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La imagen del contenedor se escanea en CI con una herramienta de costo $0. | T069 |
| `CA-02` | Un hallazgo crítico o alto sin resolver bloquea el pipeline. | T069 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T069 | Nueva | Propuesto | Integrar el escaneo de la imagen de contenedor |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
