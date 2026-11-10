# Spec — pm-165 account-deletion-web-confirm

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/account-deletion/confirm` elimina la cuenta con el token del enlace.

## 2. Dependencias

- [pm-164](../pm-164-account-deletion-web-request/spec.md) — account-deletion-web-request
- [pm-161](../pm-161-account-delete-request/spec.md) — account-delete-request

## 3. Origen

- Fuentes: Documento v3: RF-39, HU-036.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-165#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un token vigente elimina la cuenta con el mismo efecto que pm-161, sin pedir contraseña. | T183 |
| `CA-02` | Un token usado, vencido o inexistente se rechaza con el mismo error. | T183 |
| `CA-03` | Se revocan todas las sesiones de la cuenta. | T183 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T183 | Nueva | Propuesto | Confirmar la eliminación de la cuenta desde la web |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
