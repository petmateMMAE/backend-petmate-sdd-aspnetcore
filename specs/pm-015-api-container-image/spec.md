# Spec — pm-015 api-container-image

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Imagen multi-stage de la API, usuario no root y sin secretos.

## 2. Dependencias

- [pm-001](../pm-001-solution-structure/spec.md) — solution-structure
- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-008; task T011.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §3, §8.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-015#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El Dockerfile compila y publica la API en una imagen multi-stage. [origen: 00-foundation CA-008] | T046 |
| `CA-02` | La imagen ejecuta con usuario no root. | T046 |
| `CA-03` | La imagen no contiene secretos (`.dockerignore`). | T046 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T046 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de la imagen del contenedor |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
