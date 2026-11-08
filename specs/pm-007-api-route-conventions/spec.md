# Spec — pm-007 api-route-conventions

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Patrón `/api/v1/{recurso}/{acción}` y su verificación automática.

## 2. Dependencias

- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition

## 3. Origen

- Fuentes: 00-foundation/task.md T007 (evidencia parcial); constitución §7.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-007#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los endpoints de negocio usan la base versionada `/api/v1` y la alternativa no versionada no se mapea. | T042 |
| `CA-02` | La acción aparece de forma literal en la ruta (`/api/v1/{recurso}/{acción}`). | T042 |
| `CA-03` | La ruta técnica `mediator-probe` está identificada como no funcional de negocio. | T042 |
| `CA-04` | Una verificación automática comprueba que todas las rutas del código coinciden con `docs/api/endpoints.md`. | T055 |
| `CA-05` | Las reglas de nombrado de recursos y acciones aplicables a recursos futuros están documentadas y verificadas. | T055 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T042 | Retrospectiva | En progreso (retrospectiva documentada; pendiente de revisión y aprobación humana) | Documentar la evidencia retrospectiva de rutas versionadas |
| T055 | Nueva | En progreso (implementada y verificada; pendiente de aprobación humana) | Verificar automáticamente rutas contra el catálogo y documentar reglas de nombrado |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- T007 conserva su alcance histórico (OpenAPI/Scalar) y su Spec principal es pm-008; no demuestra por sí sola los criterios de rutas. T042 (retrospectiva) documenta la evidencia existente de CA-01 a CA-03 y T055 (nueva) cubre CA-04 y CA-05 sin modificar retrospectivamente T007.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
