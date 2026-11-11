# Spec — pm-144 chat-send-message

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L15 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/messages/send` envía un mensaje a una conversación.

## 2. Dependencias

- [pm-143](../pm-143-chat-conversation-open/spec.md) — chat-conversation-open

## 3. Origen

- Fuentes: Maqueta `chat`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-144#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un participante envía un mensaje de texto con una longitud máxima. | T162 |
| `CA-02` | Quien no es participante recibe 404. | T162 |
| `CA-03` | Una conversación cerrada rechaza el mensaje. | T162 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T162 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Enviar un mensaje de texto |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
