# Spec — pm-149 chat-read-state

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L17 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/conversations/mark-read` marca como leídos los mensajes de una conversación.

## 2. Dependencias

- [pm-145](../pm-145-chat-list-messages/spec.md) — chat-list-messages

## 3. Origen

- Fuentes: Maqueta `chats`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-149#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cada participante marca como leídos los mensajes hasta uno dado. | T167 |
| `CA-02` | El estado de lectura de un participante no afecta al del otro. | T167 |
| `CA-03` | El listado de conversaciones informa el número de mensajes sin leer. | T167 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T167 | Nueva | Propuesto | Llevar el estado de lectura del chat |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
