# Spec — pm-100 deworming-delete-own

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`DELETE /api/v1/dewormings/delete/{id}` borra una desparasitación que registró el propio dueño.

## 2. Dependencias

- [pm-099](../pm-099-deworming-create-list/spec.md) — deworming-create-list

## 3. Origen

- Fuentes: Documento v3: RF-15, HU-014.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-100#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño borra una desparasitación que él mismo registró. | T118 |
| `CA-02` | Una desparasitación registrada por un negocio no se borra y la respuesta indica que la registró el negocio. | T118 |
| `CA-03` | Una desparasitación de una mascota ajena o inexistente responde 404. | T118 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T118 | Nueva | Propuesto | Borrar las desparasitaciones registradas por el propio dueño |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
