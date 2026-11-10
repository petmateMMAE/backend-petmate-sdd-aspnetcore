# Spec — pm-147 chat-close-reopen

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L15 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

La conversación se cierra al dar de alta el seguimiento y la veterinaria puede reabrirla.

## 2. Dependencias

- [pm-143](../pm-143-chat-conversation-open/spec.md) — chat-conversation-open
- [pm-139](../pm-139-followup-update/spec.md) — followup-update

## 3. Origen

- Fuentes: Maqueta `chat`, `visit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-147#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Al marcar Dado de alta la conversación se cierra y rechaza mensajes nuevos. | T165 |
| `CA-02` | La veterinaria puede reabrirla y el seguimiento vuelve a En tratamiento. | T165 |
| `CA-03` | El dueño no puede reabrirla. | T165 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T165 | Nueva | Propuesto | Cerrar y reabrir la conversación |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
