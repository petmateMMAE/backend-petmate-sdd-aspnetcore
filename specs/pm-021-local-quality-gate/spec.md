# Spec — pm-021 local-quality-gate

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Orquestación local de restore, build, formato, auditorías y suites.

## 2. Dependencias

- [pm-018](../pm-018-static-analysis-sast/spec.md) — static-analysis-sast
- [pm-019](../pm-019-secret-scanning/spec.md) — secret-scanning
- [pm-020](../pm-020-dependency-license-audit/spec.md) — dependency-license-audit

## 3. Origen

- Fuentes: 00-foundation/task.md T012; `scripts/quality-gate.ps1`.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §9, §10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-021#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un único comando local ejecuta restore, build, formato, auditorías y suites. | T051 |
| `CA-02` | El gate no omite escáneres de forma silenciosa. | T051 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T051 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del gate de calidad local |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
