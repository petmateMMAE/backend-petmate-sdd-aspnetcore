# Spec — pm-155 reminder-dispatch

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Un trabajo del servidor envía por push los recordatorios que ya corresponden.

## 2. Dependencias

- [pm-154](../pm-154-reminder-generation/spec.md) — reminder-generation
- [pm-151](../pm-151-device-token-register/spec.md) — device-token-register
- [pm-153](../pm-153-push-provider-adapter/spec.md) — push-provider-adapter

## 3. Origen

- Fuentes: Documento v3: RF-19, HU-018.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-155#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El trabajo envía los recordatorios vencidos al dispositivo del dueño. | T173 |
| `CA-02` | Una reclamación atómica evita envíos duplicados con dos o más instancias. | T173 |
| `CA-03` | Solo se envía si el dueño tiene el aviso activo. | T173 |
| `CA-04` | El aviso nombra la mascota y la vacuna o desparasitación. | T173 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T173 | Nueva | Propuesto | Enviar los recordatorios vencidos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
