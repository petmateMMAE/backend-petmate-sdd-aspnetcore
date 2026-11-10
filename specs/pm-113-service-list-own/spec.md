# Spec — pm-113 service-list-own

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/services/get-all` devuelve los servicios del negocio asociado a la cuenta.

## 2. Dependencias

- [pm-112](../pm-112-service-create/spec.md) — service-create

## 3. Origen

- Fuentes: Documento v3: RF-32, HU-030.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-113#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado obtiene la lista de los servicios de su negocio. | T131 |
| `CA-02` | Una cuenta sin negocio recibe un error sin datos. | T131 |
| `CA-03` | La lista tiene un orden estable y definido. | T131 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T131 | Nueva | Propuesto | Listar los servicios del negocio propio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
