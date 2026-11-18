# Spec — pm-150 chat-realtime-channel

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L16 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Mientras una conversación está abierta, la app recibe los mensajes nuevos del otro participante consultando cada pocos segundos solo los posteriores al último recibido (enmienda del 2026-11-17: sustituye al canal en vivo por CA-04).

## 2. Dependencias

- [pm-144](../pm-144-chat-send-message/spec.md) — chat-send-message

## 3. Origen

- Fuentes: Maqueta `chat`; decisión del usuario del 2026-11-09 (tiempo real de preferencia; si es muy complejo, basta con consultar al abrir).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §3, §5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-150#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un participante autenticado obtiene los mensajes nuevos de su conversación con `GET /api/v1/messages/get-all?conversationId=&after=<último mensaje>`, que devuelve solo los posteriores y en orden. | T168 |
| `CA-02` | La consulta valida la identidad y la pertenencia a la conversación; un tercero recibe 404. | T168 |
| `CA-03` | Con dos o más instancias no se pierden ni se duplican mensajes, porque el estado vive en Atlas y no en la instancia. | T168 |
| `CA-04` | Se descarta el canal en vivo (WebSocket/SSE con Redis Pub/Sub) por costo y complejidad; el participante con la conversación cerrada se entera por push (pm-159). | T168 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T168 | Nueva | Cerrado y aprobado (2026-11-18, aprobación humana en el chat, sin revisión independiente; avisos aceptados, ver Memoria.md) | Entregar los mensajes en tiempo real |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Decisión del usuario (2026-11-17, chat): se aprueba el reemplazo recomendado en `plan.md`. Sin cambios de código ni de contrato: reutiliza pm-145.
- Con varias instancias el canal en vivo exigiría un respaldo compartido (por ejemplo, un Redis) y una decisión de la Constitución §5; el plan lo evaluará.

- Enmienda del 2026-11-18: el objetivo y CA-04 ya reflejan la consulta con `after` en lugar del canal en vivo; el título «tiempo real» de T168 y el slug del Spec se conservan por la inmutabilidad de identificadores (§12.3).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
