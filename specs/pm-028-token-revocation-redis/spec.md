# Spec — pm-028 token-revocation-redis

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Lista negra de `jti` en Redis Auth con TTL y falla cerrada.

## 2. Dependencias

- [pm-013](../pm-013-redis-dual-connectivity/spec.md) — redis-dual-connectivity
- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-007..009; task T017.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-12 (ver [traceability.md](../traceability.md)).
- Constitución: §5, §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-028#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El `jti` revocado se registra en `jwt:blacklist:{jti}` de Redis Auth con el TTL restante. [origen: 01-initial-authentication CA-007] | T017 |
| `CA-02` | Un JWT en la lista negra es rechazado aunque no haya expirado. [origen: CA-008] | T017 |
| `CA-03` | Si Redis Auth no está disponible no se concede acceso (fail-closed). [origen: CA-009] | T017 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T017 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Blacklist Redis Auth |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
