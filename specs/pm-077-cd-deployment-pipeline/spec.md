# Spec — pm-077 cd-deployment-pipeline

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Despliegue continuo versionado con humo posterior y reversa.

## 2. Dependencias

- [pm-055](../pm-055-ci-pull-request-pipeline/spec.md) — ci-pull-request-pipeline
- [pm-057](../pm-057-container-image-scan/spec.md) — container-image-scan
- [pm-058](../pm-058-sbom-generation/spec.md) — sbom-generation
- [pm-059](../pm-059-release-versioning/spec.md) — release-versioning
- [pm-073](../pm-073-public-https-endpoint/spec.md) — public-https-endpoint
- [pm-075](../pm-075-backend-two-instances/spec.md) — backend-two-instances
- [pm-076](../pm-076-load-balancer/spec.md) — load-balancer

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-07, AC-09 (ver [traceability.md](../traceability.md)).
- Constitución: §8.13.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-077#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cada despliegue corresponde a una versión trazable (SemVer, etiqueta e imagen inmutable). | T089 |
| `CA-02` | Una prueba de humo posterior al despliegue se ejecuta por HTTPS. | T089 |
| `CA-03` | Existe una vía de reversa. | T089 |
| `CA-04` | El despliegue está condicionado a la revisión de costo y a las aprobaciones de release. | T089 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T089 | Nueva | Propuesto | Implementar el pipeline de CD |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
