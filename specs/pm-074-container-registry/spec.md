# Spec — pm-074 container-registry

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Azure Container Registry (o alternativa validada dentro de Azure) con etiquetas inmutables y trazabilidad imagen-versión.

## 2. Dependencias

- [pm-059](../pm-059-release-versioning/spec.md) — release-versioning
- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-09 (ver [traceability.md](../traceability.md)).
- Constitución: §8.12.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-074#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El registro de contenedores es Azure Container Registry o una alternativa validada dentro de Azure. | T086 |
| `CA-02` | Las etiquetas de imagen son inmutables. | T086 |
| `CA-03` | Cada imagen es trazable a su versión y despliegue. | T086 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T086 | Nueva | Propuesto | Configurar el registro de contenedores |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
