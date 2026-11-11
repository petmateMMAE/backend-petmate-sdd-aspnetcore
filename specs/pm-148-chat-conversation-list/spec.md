# Spec — pm-148 chat-conversation-list

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L15 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/conversations/get-all` lista las conversaciones del usuario con su último mensaje.

## 2. Dependencias

- [pm-143](../pm-143-chat-conversation-open/spec.md) — chat-conversation-open

## 3. Origen

- Fuentes: Maqueta `chats`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-148#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño y la veterinaria listan sus conversaciones con el último mensaje y su fecha. | T166 |
| `CA-02` | Solo aparecen las conversaciones propias. | T166 |
| `CA-03` | El listado usa la convención de paginación. | T166 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T166 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Listar las conversaciones propias |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
