# Spec — pm-039 pet-domain-persistence

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Modelo Pet, validadores, persistencia e índice por propietario.

## 2. Dependencias

- [pm-012](../pm-012-atlas-connectivity/spec.md) — atlas-connectivity
- [pm-033](../pm-033-authorization-policies/spec.md) — authorization-policies

## 3. Origen

- Fuentes: 03-pet-management/spec.md; tasks T029, T036, T037.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-039#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El modelo Pet tiene validadores de especie, fechas, textos y rangos. | T029 |
| `CA-02` | La colección tiene un índice por `OwnerId`. | T029 |
| `CA-03` | La eliminación lógica se modela con un campo de estado (`IsDeleted` o equivalente). | T029 |
| `CA-04` | Las pruebas unitarias del modelo pasan. | T036 |
| `CA-05` | Las pruebas de integración contra Atlas de pruebas (índices y filtros por propietario) pasan. | T037 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T029 | Histórica | Propuesto | Definir modelo Pet y persistencia |
| T036 | Histórica | Propuesto | Pruebas unitarias Pet |
| T037 | Histórica | Propuesto | Pruebas de integración Atlas |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Decisión pendiente: campos definitivos de mascota (contrastar con maqueta móvil).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
