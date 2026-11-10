# Spec — pm-032 user-logout

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`POST /api/v1/auth/logout` que revoca sesión y `jti`.

## 2. Dependencias

- [pm-028](../pm-028-token-revocation-redis/spec.md) — token-revocation-redis
- [pm-031](../pm-031-refresh-token-rotation/spec.md) — refresh-token-rotation

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-007; task T021.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-032#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Logout revoca la sesión en Atlas y registra el `jti` en Redis Auth. [origen: 01-initial-authentication CA-007] | T021 |
| `CA-02` | Logout es idempotente para una sesión propia ya revocada y devuelve 403 si el refresh token pertenece a otro usuario. | T021 |
| `CA-03` | Con Redis Auth o Atlas no disponibles devuelve 503 (fail-closed). | T021 |
| `CA-04` | El contrato de Logout (encabezado, cuerpo, idempotencia y token expirado) está documentado en `docs/api/endpoints.md`. | T021 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Logout |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
