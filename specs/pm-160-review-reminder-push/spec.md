# Spec — pm-160 review-reminder-push

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Un día antes de la próxima revisión del seguimiento se notifica al dueño.

## 2. Dependencias

- [pm-138](../pm-138-followup-open/spec.md) — followup-open
- [pm-155](../pm-155-reminder-dispatch/spec.md) — reminder-dispatch

## 3. Origen

- Fuentes: Maqueta `notifSettings`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-160#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un día antes de la revisión se envía un aviso al dueño que lo tenga activo. | T178 |
| `CA-02` | El aviso se cancela si el seguimiento cambia de fecha o se da de alta. | T178 |
| `CA-03` | El envío es idempotente. | T178 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T178 | Nueva | Propuesto | Avisar un día antes de la revisión |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
