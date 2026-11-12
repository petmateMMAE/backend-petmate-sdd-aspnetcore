# Spec — pm-145 chat-list-messages

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L16 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/messages/get-all` devuelve los mensajes de una conversación a partir de un cursor.

## 2. Dependencias

- [pm-144](../pm-144-chat-send-message/spec.md) — chat-send-message

## 3. Origen

- Fuentes: Maqueta `chat`; decisión del usuario del 2026-11-09 (basta con consultar al abrir si el tiempo real es muy complejo).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-145#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Devuelve los mensajes en orden y permite pedir solo los posteriores a un mensaje dado. | T163 |
| `CA-02` | Solo los participantes pueden leerlos. | T163 |
| `CA-03` | Incluye los avisos del sistema del seguimiento. | T163 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T163 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Listar los mensajes de una conversación |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
