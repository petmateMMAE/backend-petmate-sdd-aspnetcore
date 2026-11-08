# Spec — pm-061 api-docs-contract-sync

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Rutas del código iguales al catálogo y wrapper representado en OpenAPI.

## 2. Dependencias

- [pm-007](../pm-007-api-route-conventions/spec.md) — api-route-conventions
- [pm-008](../pm-008-openapi-scalar-documentation/spec.md) — openapi-scalar-documentation
- [pm-009](../pm-009-response-wrapper-contract/spec.md) — response-wrapper-contract

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T024; 03-pet-management/task.md T039.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-061#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los contratos de autenticación están documentados y sincronizados con `docs/api/endpoints.md`. | T024 |
| `CA-02` | Los contratos de mascotas están documentados y probados (pruebas de contrato) contra los endpoints de pm-040 a pm-044. | T039 |
| `CA-03` | Las rutas del código coinciden automáticamente con el catálogo. | T073 |
| `CA-04` | El documento OpenAPI representa el Response Wrapper. | T073 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T024 | Histórica | En progreso (verificada con evidencia; pendiente de aprobación humana) | Contratos y documentación de auth |
| T039 | Histórica | En progreso (verificada con evidencia; pendiente de aprobación humana) | Pruebas de contrato y documentación |
| T073 | Nueva | En progreso (implementada y verificada; pendiente de aprobación humana) | Sincronizar contratos, catálogo y OpenAPI |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- T039 es posterior a T040–T044 (dependencia entre Tasks, no entre Specs).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-08, aprobación en el chat de la sesión ("Si, apruebalos y realizalos"). Sin revisión independiente de seguridad todavía.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
