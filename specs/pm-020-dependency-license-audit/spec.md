# Spec — pm-020 dependency-license-audit

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Auditoría de vulnerabilidades, desuso y licencias NuGet con gobierno de excepciones.

## 2. Dependencias

- [pm-003](../pm-003-code-standards-central-packages/spec.md) — code-standards-central-packages

## 3. Origen

- Fuentes: 00-foundation/task.md T012; `scripts/quality-gate.ps1`.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-020#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La auditoría de vulnerabilidades y paquetes en desuso de NuGet falla ante hallazgos. | T050 |
| `CA-02` | La auditoría de licencias usa una lista permitida. | T050 |
| `CA-03` | Las excepciones tienen gobierno (`scripts/quality-exceptions.json` con justificación y vencimiento). | T050 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T050 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de la auditoría de dependencias y licencias |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
