# Spec — pm-042 pet-detail

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`GET /api/v1/pets/get-by-id/{id}` solo de mascota propia.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-003; task T032.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-042#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Owner obtiene el detalle solo de una mascota propia. [origen: 03-pet-management CA-003] | T032 |
| `CA-02` | Una mascota ajena o inexistente devuelve 404. | T032 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T032 | Histórica | Propuesto | Obtener detalle |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
