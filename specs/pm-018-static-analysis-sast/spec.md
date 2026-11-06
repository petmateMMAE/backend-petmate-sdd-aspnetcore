# Spec — pm-018 static-analysis-sast

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Reglas de seguridad de los analizadores como errores de compilación.

## 2. Dependencias

- [pm-003](../pm-003-code-standards-central-packages/spec.md) — code-standards-central-packages

## 3. Origen

- Fuentes: 00-foundation/task.md T012.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-018#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las reglas de seguridad de los analizadores se aplican como errores de compilación. | T048 |
| `CA-02` | El build Release termina con 0 advertencias. | T048 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T048 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de análisis estático |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
