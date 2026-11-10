# Spec — pm-157 notification-settings

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET` y `PUT /api/v1/notification-settings` consultan y cambian las preferencias de aviso del usuario.

## 2. Dependencias

- [pm-086](../pm-086-user-role-unification/spec.md) — user-role-unification

## 3. Origen

- Fuentes: Maqueta `notifSettings`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-157#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El usuario activa o desactiva avisos de vacunas y desparasitaciones, de revisiones, de mensajes del chat y de cambios del seguimiento, y elige el sonido. | T175 |
| `CA-02` | El usuario elige con cuántos días de anticipación se avisa dentro de un rango permitido. | T175 |
| `CA-03` | Las preferencias son propias y tienen valores por defecto. | T175 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T175 | Nueva | Propuesto | Guardar los ajustes de notificación |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La maqueta incluye un aviso de promociones de negocios cercanos que ninguna fuente define como funcionalidad; queda fuera hasta que se decida.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
