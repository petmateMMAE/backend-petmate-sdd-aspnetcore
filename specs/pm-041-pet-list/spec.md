# Spec — pm-041 pet-list

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`GET /api/v1/pets/get-all` solo de mascotas propias y activas, paginado.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence
- [pm-047](../pm-047-pagination-convention/spec.md) — pagination-convention

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-002; task T031.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-041#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Owner lista únicamente sus mascotas. [origen: 03-pet-management CA-002; sin estado «activa» porque el borrado es físico, decisión del usuario del 2026-11-08] | T031 |
| `CA-02` | El listado es paginado con el tope definido en pm-047. | T031 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T031 | Histórica | Propuesto | Listar mascotas |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
