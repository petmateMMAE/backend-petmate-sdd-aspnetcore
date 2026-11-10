# Spec — pm-030 user-login

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`POST /api/v1/auth/login` con respuesta idéntica ante credenciales inválidas.

## 2. Dependencias

- [pm-011](../pm-011-request-validation/spec.md) — request-validation
- [pm-024](../pm-024-users-persistence/spec.md) — users-persistence
- [pm-025](../pm-025-refresh-sessions-persistence/spec.md) — refresh-sessions-persistence
- [pm-026](../pm-026-password-hashing-argon2id/spec.md) — password-hashing-argon2id
- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-003; task T019; docs/api/endpoints.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-030#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Login emite access y refresh token. [origen: 01-initial-authentication CA-003] | T019 |
| `CA-02` | La respuesta 401 es idéntica para correo inexistente y contraseña incorrecta. | T019 |
| `CA-03` | Si la espera de hashing se agota la respuesta es 503. | T019 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Login |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
