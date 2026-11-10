# Spec — pm-137 clinical-history-view

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/history/get-all` devuelve el historial médico de una mascota propia con filtro por tipo.

## 2. Dependencias

- [pm-097](../pm-097-vaccine-create-list/spec.md) — vaccine-create-list
- [pm-099](../pm-099-deworming-create-list/spec.md) — deworming-create-list
- [pm-101](../pm-101-weight-create-list/spec.md) — weight-create-list
- [pm-133](../pm-133-consultation-owner-list/spec.md) — consultation-owner-list

## 3. Origen

- Fuentes: Maqueta `history`; documento v3: RF-18.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-137#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El historial mezcla consultas, vacunas, desparasitaciones y pesos de la más reciente a la más antigua. | T155 |
| `CA-02` | Admite filtrar por tipo de registro. | T155 |
| `CA-03` | Una mascota ajena o inexistente responde 404 y el listado usa la convención de paginación. | T155 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T155 | Nueva | Propuesto | Consultar el historial médico unificado |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
