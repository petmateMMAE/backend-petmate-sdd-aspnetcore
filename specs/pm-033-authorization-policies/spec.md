# Spec — pm-033 authorization-policies

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Política de autorización por defecto y por rol Owner/Business.

## 2. Dependencias

- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt
- [pm-028](../pm-028-token-revocation-redis/spec.md) — token-revocation-redis

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T022.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-033#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Existe una política de autorización por defecto que exige usuario autenticado. | T022 |
| `CA-02` | Existen políticas por rol Owner y Business. | T022 |
| `CA-03` | Un anónimo recibe 401 y un rol insuficiente recibe 403. | T022 |
| `CA-04` | El rol Business no obtiene acceso por defecto a la API de propietario. | T022 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T022 | Histórica | Propuesto | Políticas de autenticación y autorización |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Criterios propuestos; su detalle se cierra al aprobar este Spec.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
