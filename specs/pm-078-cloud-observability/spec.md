# Spec — pm-078 cloud-observability

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Eventos, errores y alertas en un servicio de observabilidad de Azure, sin PII ni secretos.

## 2. Dependencias

- [pm-054](../pm-054-structured-logging-pii-safe/spec.md) — structured-logging-pii-safe
- [pm-075](../pm-075-backend-two-instances/spec.md) — backend-two-instances

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-08 (ver [traceability.md](../traceability.md)).
- Constitución: §8.14.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-078#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los eventos y errores llegan a un servicio de observabilidad de Azure. | T090 |
| `CA-02` | Existen alertas sobre indisponibilidad y errores. | T090 |
| `CA-03` | Los datos enviados no contienen PII ni secretos. | T090 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T090 | Nueva | Propuesto | Configurar la observabilidad cloud |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
