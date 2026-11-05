# Spec — pm-067 iac-state-management

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Estado remoto de IaC con bloqueo, cifrado y sin secretos.

## 2. Dependencias

- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-10 (ver [traceability.md](../traceability.md)).
- Constitución: §8.15.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-067#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El estado de IaC se guarda de forma remota con bloqueo y cifrado. | T079 |
| `CA-02` | El estado y el repositorio no contienen secretos. | T079 |
| `CA-03` | El almacenamiento del estado no genera cargos (clasificado en pm-062). | T079 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T079 | Nueva | Propuesto | Configurar el estado remoto de IaC |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
