# Spec — pm-098 vaccine-delete-own

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`DELETE /api/v1/vaccines/delete/{id}` borra una vacuna que registró el propio dueño.

## 2. Dependencias

- [pm-097](../pm-097-vaccine-create-list/spec.md) — vaccine-create-list

## 3. Origen

- Fuentes: Documento v3: RF-14, HU-013.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-098#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño borra una vacuna que él mismo registró. | T116 |
| `CA-02` | Una vacuna registrada por un negocio no se borra y la respuesta indica que la registró el negocio. | T116 |
| `CA-03` | Una vacuna de una mascota ajena o inexistente responde 404. | T116 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T116 | Nueva | Propuesto | Borrar las vacunas registradas por el propio dueño |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
