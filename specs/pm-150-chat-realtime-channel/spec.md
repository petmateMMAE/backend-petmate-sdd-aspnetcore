# Spec — pm-150 chat-realtime-channel

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L16 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Los mensajes llegan al otro participante sin recargar mientras la conversación está abierta.

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
| `CA-01` | Un participante autenticado recibe los mensajes de sus conversaciones en cuanto se envían. | T168 |
| `CA-02` | La conexión valida la identidad y la pertenencia a la conversación. | T168 |
| `CA-03` | Con dos o más instancias no se pierden ni se duplican mensajes. | T168 |
| `CA-04` | Si el plan demuestra que no es viable dentro del costo y la complejidad aceptables, este Spec se reemplaza por la consulta al abrir (pm-145). | T168 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T168 | Nueva | Propuesto | Entregar los mensajes en tiempo real |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Con varias instancias exige un respaldo compartido (por ejemplo, un Redis) y una decisión de la Constitución §5; el plan lo evaluará.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
