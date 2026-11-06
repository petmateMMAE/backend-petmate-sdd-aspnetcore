# Spec — pm-008 openapi-scalar-documentation

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Documento OpenAPI generado y Scalar como única UI, solo en Development.

## 2. Dependencias

- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition
- [pm-007](../pm-007-api-route-conventions/spec.md) — api-route-conventions

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-010; decisions/ADR-002; task T007.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-008#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El documento `/openapi/v1.json` es válido y la UI `/scalar/v1` lo carga en Development. [origen: 00-foundation CA-010] | T007 |
| `CA-02` | No se agrega Swagger UI ni otra UI alternativa a Scalar. | T007 |
| `CA-03` | En producción el documento y la UI permanecen deshabilitados salvo habilitación explícita y revisión de seguridad. | T007 |
| `CA-04` | El esquema de seguridad Bearer (JWT) está descrito en el documento OpenAPI. | T007 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T007 | Histórica | Cerrado (2026-11-03) | Versionado, convenciones MVC, OpenAPI y Scalar |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Se conserva la brecha heredada CON-§7.3 y el pendiente de OpenAPI registrados en los gates históricos de T007; esta migración no los resuelve.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
