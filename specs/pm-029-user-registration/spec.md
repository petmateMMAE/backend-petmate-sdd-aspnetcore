# Spec — pm-029 user-registration

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`POST /api/v1/auth/register` que crea siempre un Owner.

## 2. Dependencias

- [pm-011](../pm-011-request-validation/spec.md) — request-validation
- [pm-024](../pm-024-users-persistence/spec.md) — users-persistence
- [pm-025](../pm-025-refresh-sessions-persistence/spec.md) — refresh-sessions-persistence
- [pm-026](../pm-026-password-hashing-argon2id/spec.md) — password-hashing-argon2id
- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-001; task T018; docs/api/endpoints.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-029#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Register crea un Owner y rechaza el correo duplicado con 409 y mensaje genérico. [origen: 01-initial-authentication CA-001] | T018 |
| `CA-02` | Un cuerpo con miembros desconocidos (p. ej. `role`) devuelve 400. | T018 |
| `CA-03` | La contraseña debe tener entre 12 y 128 caracteres. | T018 |
| `CA-04` | La respuesta 201 devuelve `AuthResponse` con access y refresh token. | T018 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Register |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
