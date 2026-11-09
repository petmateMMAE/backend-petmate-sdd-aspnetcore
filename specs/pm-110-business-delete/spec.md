# Spec — pm-110 business-delete

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`DELETE /api/v1/businesses/delete` elimina el negocio asociado a la cuenta.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create

## 3. Origen

- Fuentes: Documento v3: RF-28, HU-027.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.1.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-110#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado elimina su negocio y la cuenta deja de tener negocio asociado. | T128 |
| `CA-02` | El negocio deja de aparecer en las consultas públicas y en el mapa. | T128 |
| `CA-03` | La cuenta y sus mascotas siguen funcionando. | T128 |
| `CA-04` | Las consultas que el negocio registró como veterinaria se conservan en el carnet de los dueños. | T128 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T128 | Nueva | Propuesto | Dar de baja el negocio propio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
