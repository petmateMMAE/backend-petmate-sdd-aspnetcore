# Spec — pm-076 load-balancer

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Balanceador de carga con reparto verificable.

## 2. Dependencias

- [pm-075](../pm-075-backend-two-instances/spec.md) — backend-two-instances

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-04 (ver [traceability.md](../traceability.md)).
- Constitución: §8.3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-076#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Load Balancer o servicio equivalente de Azure se sitúa delante de las instancias. | T088 |
| `CA-02` | El reparto entre instancias es verificable. | T088 |
| `CA-03` | Retirar una instancia no interrumpe el servicio. | T088 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T088 | Nueva | Propuesto | Configurar el Load Balancer |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
