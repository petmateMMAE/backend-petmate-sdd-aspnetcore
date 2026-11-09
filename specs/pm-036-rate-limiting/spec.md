# Spec — pm-036 rate-limiting

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Límites por política y por IP, y matriz de límites para los endpoints no autenticación (documentada en `docs/api/endpoints.md`).

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
| `CA-05` | El límite por IP de `/api/v1/auth/*` se comparte entre réplicas mediante Redis Auth y falla cerrado (503) si Redis no está disponible. | T100 |
| `CA-06` | En Production la ausencia de `TRUSTED_PROXIES` se advierte al arrancar (y es fatal con `REQUIRE_TRUSTED_PROXIES=true`); el número de saltos de red y `ForwardLimit = 1` están documentados. | T100 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T054 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del rate limiting de autenticación |
| T057 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir la matriz de rate limiting para endpoints no autenticación |
| T100 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | Límite por IP distribuido en Redis Auth, validación de proxies al arrancar y documentación de saltos (H6) |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Matriz de límites aprobada (2026-11-08):** se documenta en `docs/api/endpoints.md` (sección «Matriz de límites de tasa») y cubre todos los endpoints actuales. Decisiones: el perfil propio (`users/me/*`) y las mascotas llevan 60 peticiones por minuto por usuario; `health/live` y `health/ready` y el endpoint técnico quedan sin límite (sondas del balanceador y endpoint temporal público de pm-033 P-04); `logout` usa la misma política por usuario (60 por minuto) desde la revisión de seguridad del grupo de autenticación (H5), pese a exigir un token válido.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
- Enmienda 2026-11-08 (T100, hallazgo H6): Aprobado por el usuario el 2026-11-08 (opciones A de H7, H3 y H8; B de H4; A, B y C de H6).
