# Spec — pm-031 refresh-token-rotation

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`POST /api/v1/auth/refresh` con rotación atómica y detección de replay.

## 2. Dependencias

- [pm-025](../pm-025-refresh-sessions-persistence/spec.md) — refresh-sessions-persistence
- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt
- [pm-030](../pm-030-user-login/spec.md) — user-login

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-004..006; task T020.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-031#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Refresh rota el token y el anterior no puede reutilizarse. [origen: 01-initial-authentication CA-005] | T020 |
| `CA-02` | El replay de un token consumido se detecta y revoca la sesión. [origen: CA-006] | T020 |
| `CA-03` | Un access token expirado puede renovarse mientras el refresh siga vigente. [origen: CA-004] | T020 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Refresh Token persistente y rotatorio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
