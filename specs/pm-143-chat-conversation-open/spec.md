# Spec — pm-143 chat-conversation-open

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Al abrir un seguimiento existe una conversación entre el dueño y la veterinaria de esa atención.

## 2. Dependencias

- [pm-138](../pm-138-followup-open/spec.md) — followup-open

## 3. Origen

- Fuentes: Maqueta `chat`; decisión del usuario del 2026-11-09 (el chat entra).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-143#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Abrir un seguimiento crea una conversación con exactamente dos participantes: el dueño y la veterinaria. | T161 |
| `CA-02` | Hay una sola conversación por seguimiento. | T161 |
| `CA-03` | Ningún otro usuario puede leerla ni escribir en ella. | T161 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T161 | Nueva | Propuesto | Abrir la conversación de un seguimiento |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
