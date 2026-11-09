# Spec — pm-166 openapi-contract-artifact

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Cada ejecución de CI genera el documento OpenAPI y lo publica como artefacto para el equipo móvil.

## 2. Dependencias

- [pm-061](../pm-061-api-docs-contract-sync/spec.md) — api-docs-contract-sync
- [pm-008](../pm-008-openapi-scalar-documentation/spec.md) — openapi-scalar-documentation

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09 (flujo de integración con la app).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.3, §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-166#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | CI genera `openapi.json` en cada build y lo publica como artefacto. | T184 |
| `CA-02` | El documento incluye el Response Wrapper en todas las respuestas. | T184 |
| `CA-03` | En Production el OpenAPI y Scalar siguen ocultos. | T184 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T184 | Nueva | Propuesto | Publicar el contrato OpenAPI como artefacto de CI |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
