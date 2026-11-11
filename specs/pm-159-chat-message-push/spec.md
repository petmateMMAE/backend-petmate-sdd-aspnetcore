# Spec — pm-159 chat-message-push

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L16 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Un mensaje nuevo en el chat notifica por push al otro participante.

## 2. Dependencias

- [pm-144](../pm-144-chat-send-message/spec.md) — chat-send-message
- [pm-155](../pm-155-reminder-dispatch/spec.md) — reminder-dispatch

## 3. Origen

- Fuentes: Maqueta `notifSettings`, `chat`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-159#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Enviar un mensaje avisa al otro participante, salvo que haya desactivado ese aviso. | T177 |
| `CA-02` | El aviso no incluye diagnósticos ni recetas. | T177 |
| `CA-03` | Un mismo mensaje no envía avisos duplicados. | T177 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T177 | Nueva | Aprobada para iniciar (2026-11-10, aprobación humana; pendiente de plan.md) | Avisar al otro participante de un mensaje nuevo |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
