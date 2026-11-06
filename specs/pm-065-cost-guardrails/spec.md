# Spec — pm-065 cost-guardrails

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Presupuesto, alertas y etiquetado de costos.

## 2. Dependencias

- [pm-063](../pm-063-cloud-provider-selection/spec.md) — cloud-provider-selection

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-02 (ver [traceability.md](../traceability.md)).
- Constitución: §8.2, §8.5, §8.6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-065#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Presupuesto y alertas de Azure Cost Management están definidos (su creación real requiere autorización expresa). | T077 |
| `CA-02` | Todos los recursos se etiquetan para seguimiento de costo. | T077 |
| `CA-03` | Existe un procedimiento ante el agotamiento de créditos que no recurre a una sola instancia. | T077 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T077 | Nueva | Propuesto | Definir presupuesto, alertas y etiquetado de costos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
