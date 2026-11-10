# Spec — pm-140 followup-notes

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/followups/add-note` agrega una nota con fecha al seguimiento.

## 2. Dependencias

- [pm-138](../pm-138-followup-open/spec.md) — followup-open

## 3. Origen

- Fuentes: Maqueta `visit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-140#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La veterinaria autora agrega una nota con fecha. | T158 |
| `CA-02` | Las notas se conservan en orden cronológico. | T158 |
| `CA-03` | Solo la veterinaria autora puede agregarlas. | T158 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T158 | Nueva | Propuesto | Agregar notas al seguimiento |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
