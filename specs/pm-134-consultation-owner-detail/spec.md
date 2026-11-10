# Spec — pm-134 consultation-owner-detail

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/consultations/get-by-id/{id}` devuelve una consulta con su receta completa.

## 2. Dependencias

- [pm-133](../pm-133-consultation-owner-list/spec.md) — consultation-owner-list
- [pm-132](../pm-132-prescription-items/spec.md) — prescription-items

## 3. Origen

- Fuentes: Documento v3: RF-18, HU-017; maqueta `consult`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-134#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño obtiene la consulta con todos los medicamentos de su receta. | T152 |
| `CA-02` | Una consulta de una mascota ajena o inexistente responde 404. | T152 |
| `CA-03` | La consulta es de solo lectura. | T152 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T152 | Nueva | Propuesto | Consultar el detalle de una consulta propia |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
