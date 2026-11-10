# Spec — pm-107 business-affiliate-create

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/businesses/create` asocia un negocio a la cuenta del usuario autenticado, sin aprobación manual.

## 2. Dependencias

- [pm-105](../pm-105-business-persistence-model/spec.md) — business-persistence-model
- [pm-106](../pm-106-business-schedule-model/spec.md) — business-schedule-model
- [pm-087](../pm-087-session-business-link/spec.md) — session-business-link

## 3. Origen

- Fuentes: Documento v3: RF-25, RF-27, HU-023, HU-025; maqueta `affiliate`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.1.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-107#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un usuario sin negocio lo crea con los datos obligatorios; la cédula profesional es obligatoria solo para Veterinaria y no se valida. | T125 |
| `CA-02` | Un usuario que ya tiene un negocio recibe 409. | T125 |
| `CA-03` | El negocio queda asociado a la cuenta, que conserva su rol y sus mascotas, y aparece en las consultas públicas sin aprobación manual. | T125 |
| `CA-04` | La respuesta de sesión informa el tipo del negocio asociado (completa pm-087#CA-02). | T125 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T125 | Nueva | Propuesto | Afiliar el negocio de un usuario |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
