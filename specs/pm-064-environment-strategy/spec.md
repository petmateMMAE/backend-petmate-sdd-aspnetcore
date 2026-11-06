# Spec — pm-064 environment-strategy

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Entornos, bases por entorno y convenciones de nombres.

## 2. Dependencias

- [pm-063](../pm-063-cloud-provider-selection/spec.md) — cloud-provider-selection

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-10 (ver [traceability.md](../traceability.md)).
- Constitución: §8, §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-064#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los entornos (desarrollo, pruebas, demo) y sus diferencias están definidos. | T076 |
| `CA-02` | Cada entorno usa una base de datos de Atlas distinta. | T076 |
| `CA-03` | Existen convenciones de nombres y etiquetas de recursos Azure. | T076 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T076 | Nueva | Propuesto | Definir la estrategia de entornos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
