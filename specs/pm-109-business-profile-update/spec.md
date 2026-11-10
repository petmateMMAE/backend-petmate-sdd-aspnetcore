# Spec — pm-109 business-profile-update

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`PUT /api/v1/businesses/update` edita el nombre, la descripción, los teléfonos y el horario del negocio propio.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create

## 3. Origen

- Fuentes: Documento v3: RF-30, HU-029; maqueta `bizEdit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-109#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado edita nombre, descripción, teléfono, WhatsApp y horario. | T127 |
| `CA-02` | El tipo de negocio no se puede cambiar y la solicitud que lo intente devuelve 400. | T127 |
| `CA-03` | Solo el dueño del negocio puede editarlo. | T127 |
| `CA-04` | Los cambios se reflejan en las consultas públicas. | T127 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T127 | Nueva | Propuesto | Actualizar los datos del negocio propio |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Cambiar el tipo exige dar de baja el negocio y afiliarlo de nuevo.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
