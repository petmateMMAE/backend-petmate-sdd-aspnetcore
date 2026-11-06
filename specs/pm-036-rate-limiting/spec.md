# Spec — pm-036 rate-limiting

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Límites por política y por IP, auth ya implementado y matriz global pendiente.

## 2. Dependencias

- [pm-017](../pm-017-trusted-proxy-forwarded-headers/spec.md) — trusted-proxy-forwarded-headers
- [pm-029](../pm-029-user-registration/spec.md) — user-registration
- [pm-030](../pm-030-user-login/spec.md) — user-login
- [pm-031](../pm-031-refresh-token-rotation/spec.md) — refresh-token-rotation

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T018–T020; `RateLimitAndProxyTests`.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-036#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Register, Login y Refresh están limitados por IP (`AUTH_RATE_LIMIT_PER_MINUTE`, por defecto 10 por minuto) con 429. | T054 |
| `CA-02` | La clave de partición usa la IP real (IPv6 agrupada por /64). | T054 |
| `CA-03` | Existe una matriz de límites para los endpoints no autenticación. | T057 |
| `CA-04` | Las respuestas 429 usan el wrapper y `Retry-After`. | T054 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T054 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del rate limiting de autenticación |
| T057 | Nueva | Propuesto | Definir la matriz de rate limiting para endpoints no autenticación |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
