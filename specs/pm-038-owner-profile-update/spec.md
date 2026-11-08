# Spec — pm-038 owner-profile-update

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`PUT /api/v1/users/me/update` con campos aprobados.

## 2. Dependencias

- [pm-037](../pm-037-owner-profile-read/spec.md) — owner-profile-read

## 3. Origen

- Fuentes: 02-owner-profile/spec.md; tasks T027, T028.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-038#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | `PUT /api/v1/users/me/update` actualiza únicamente los campos aprobados. | T027 |
| `CA-02` | Los campos no editables y los miembros desconocidos se rechazan con 400. | T027 |
| `CA-03` | Las pruebas unitarias, de integración, de seguridad y de contrato del perfil (pm-037 y pm-038) pasan y los gates tienen evidencia. | T028 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T027 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar PUT perfil propio |
| T028 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas y gates de perfil |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Decisión pendiente: campos editables del perfil.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
