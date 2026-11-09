# Spec — pm-090 password-reset-confirm

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/auth/reset-password` fija una contraseña nueva con un token de restablecimiento válido.

## 2. Dependencias

- [pm-089](../pm-089-password-reset-request/spec.md) — password-reset-request
- [pm-026](../pm-026-password-hashing-argon2id/spec.md) — password-hashing-argon2id
- [pm-025](../pm-025-refresh-sessions-persistence/spec.md) — refresh-sessions-persistence
- [pm-028](../pm-028-token-revocation-redis/spec.md) — token-revocation-redis

## 3. Origen

- Fuentes: Documento v3: RF-04, HU-003.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.2, §6.4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-090#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un token vigente permite fijar una contraseña nueva que cumple la política vigente. | T108 |
| `CA-02` | El token se invalida al usarse y un token vencido, usado o inexistente se rechaza con el mismo error. | T108 |
| `CA-03` | Al cambiar la contraseña se revocan todas las sesiones activas de la cuenta. | T108 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T108 | Nueva | Propuesto | Restablecer la contraseña con el enlace |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
