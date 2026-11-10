# Spec — pm-158 followup-change-push

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L15 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Un cambio de estado o de revisión del seguimiento notifica al dueño por push.

## 2. Dependencias

- [pm-139](../pm-139-followup-update/spec.md) — followup-update
- [pm-155](../pm-155-reminder-dispatch/spec.md) — reminder-dispatch

## 3. Origen

- Fuentes: Maqueta `notifSettings`, `visit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-158#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cambiar el estado o la próxima revisión envía un aviso al dueño. | T176 |
| `CA-02` | El aviso respeta el ajuste del dueño. | T176 |
| `CA-03` | Un mismo cambio no envía avisos duplicados. | T176 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T176 | Nueva | Propuesto | Avisar al dueño de cambios en el seguimiento |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
