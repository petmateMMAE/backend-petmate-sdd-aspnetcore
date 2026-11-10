# Spec — pm-046 first-milestone-acceptance

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Aceptación de extremo a extremo de los 11 endpoints del primer hito.

## 2. Dependencias

- [pm-035](../pm-035-auth-session-lifecycle-acceptance/spec.md) — auth-session-lifecycle-acceptance
- [pm-038](../pm-038-owner-profile-update/spec.md) — owner-profile-update
- [pm-045](../pm-045-pet-ownership-bola/spec.md) — pet-ownership-bola

## 3. Origen

- Fuentes: 04-first-milestone-closure/spec.md; task T041.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-046#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los cuatro endpoints de autenticación y los siete endpoints funcionales están operativos, integrados, probados y pasan los gates. | T041 |
| `CA-02` | El hito recibe aprobación humana explícita. | T041 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T041 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Validación end-to-end y aceptación del primer hito |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El hito se cumple solo al cerrar T041 con todos los gates y aprobaciones.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
