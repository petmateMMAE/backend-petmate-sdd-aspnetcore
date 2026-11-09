# Spec — pm-119 business-public-profile

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/businesses/get-by-id/{id}` devuelve el perfil público de un negocio, también sin cuenta.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create
- [pm-112](../pm-112-service-create/spec.md) — service-create
- [pm-106](../pm-106-business-schedule-model/spec.md) — business-schedule-model

## 3. Origen

- Fuentes: Documento v3: RF-23, HU-022; maqueta `bizProfile`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-119#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El perfil incluye descripción, foto, horario, ubicación, servicios y datos de contacto públicos. | T137 |
| `CA-02` | El perfil nunca incluye la cédula profesional. | T137 |
| `CA-03` | Un dato de contacto ausente se devuelve nulo para que la app no muestre su botón. | T137 |
| `CA-04` | Un negocio inexistente o dado de baja responde 404. | T137 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T137 | Nueva | Propuesto | Consultar el perfil público de un negocio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
