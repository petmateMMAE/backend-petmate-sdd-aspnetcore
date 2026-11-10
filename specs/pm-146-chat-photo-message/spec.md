# Spec — pm-146 chat-photo-message

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L16 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Un mensaje puede llevar una foto guardada en Blob privado.

## 2. Dependencias

- [pm-144](../pm-144-chat-send-message/spec.md) — chat-send-message
- [pm-079](../pm-079-image-storage-adapter/spec.md) — image-storage-adapter

## 3. Origen

- Fuentes: Maqueta `chat`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §8.10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-146#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un mensaje admite una foto de hasta 1 MB y de tipo de imagen permitido. | T164 |
| `CA-02` | La foto es privada y solo los participantes la ven mediante una URL temporal. | T164 |
| `CA-03` | La foto se elimina con la conversación. | T164 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T164 | Nueva | Propuesto | Enviar una foto en el chat |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
