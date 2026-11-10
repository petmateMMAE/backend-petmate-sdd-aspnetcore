# Spec — pm-152 device-token-unregister

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/devices/unregister` borra el token de un dispositivo.

## 2. Dependencias

- [pm-151](../pm-151-device-token-register/spec.md) — device-token-register

## 3. Origen

- Fuentes: Documento v3: RF-05.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-152#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un usuario quita el token de su dispositivo al cerrar sesión. | T170 |
| `CA-02` | Los tokens que el proveedor reporta como inválidos se eliminan. | T170 |
| `CA-03` | Los tokens se borran al eliminar la cuenta. | T170 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T170 | Nueva | Propuesto | Quitar el dispositivo al cerrar sesión |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
