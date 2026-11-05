# Spec — pm-037 owner-profile-read

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`GET /api/v1/users/me/get` del usuario autenticado.

## 2. Dependencias

- [pm-024](../pm-024-users-persistence/spec.md) — users-persistence
- [pm-033](../pm-033-authorization-policies/spec.md) — authorization-policies

## 3. Origen

- Fuentes: 02-owner-profile/spec.md; task T026.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-037#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | `GET /api/v1/users/me/get` devuelve el perfil del usuario autenticado. | T026 |
| `CA-02` | Solo se devuelven datos propios; un anónimo recibe 401. | T026 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T026 | Histórica | Propuesto | Implementar GET perfil propio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
