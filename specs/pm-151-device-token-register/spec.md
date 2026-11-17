# Spec — pm-151 device-token-register

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/devices/register` guarda el token de notificaciones del dispositivo del usuario.

## 2. Dependencias

- [pm-086](../pm-086-user-role-unification/spec.md) — user-role-unification

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09 (push desde el servidor).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-151#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un usuario registra el token de su dispositivo. | T169 |
| `CA-02` | El mismo token no se duplica y un usuario tiene un máximo de dispositivos. | T169 |
| `CA-03` | El token nunca se escribe en logs. | T169 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T169 | Nueva | Aprobada para iniciar (2026-11-10, aprobación humana; pendiente de plan.md) | Registrar el dispositivo para recibir avisos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
