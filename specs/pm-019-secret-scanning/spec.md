# Spec — pm-019 secret-scanning

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Detección de secretos en archivos versionables con controles positivos y negativos.

## 2. Dependencias

- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets

## 3. Origen

- Fuentes: 00-foundation/task.md T012.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-019#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El escaneo detecta secretos en archivos versionables. | T049 |
| `CA-02` | Existen controles positivo y negativo que demuestran que el escáner funciona (`SecretScanTests`). | T049 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T049 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del escaneo de secretos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
