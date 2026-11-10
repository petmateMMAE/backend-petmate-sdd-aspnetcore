# Spec — pm-163 account-delete-business-data

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Al eliminar la cuenta se borra el negocio asociado y sus datos.

## 2. Dependencias

- [pm-161](../pm-161-account-delete-request/spec.md) — account-delete-request
- [pm-110](../pm-110-business-delete/spec.md) — business-delete
- [pm-115](../pm-115-service-delete/spec.md) — service-delete
- [pm-129](../pm-129-scan-log-record/spec.md) — scan-log-record

## 3. Origen

- Fuentes: Documento v3: RNF-08, HU-006.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-163#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Se borran el negocio, sus servicios, su foto pública y sus registros de escaneo. | T181 |
| `CA-02` | El negocio deja de aparecer en el mapa y la caché pública se invalida. | T181 |
| `CA-03` | Las consultas que el negocio registró en carnets ajenos se conservan. | T181 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T181 | Nueva | Propuesto | Borrar el negocio al eliminar la cuenta |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
