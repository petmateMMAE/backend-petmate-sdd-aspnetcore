# Spec — pm-056 test-coverage-baseline

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Línea base y umbral de cobertura de pruebas.

## 2. Dependencias

- [pm-021](../pm-021-local-quality-gate/spec.md) — local-quality-gate

## 3. Origen

- Fuentes: nuevo (constitución §10).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-06 (ver [traceability.md](../traceability.md)).
- Constitución: §10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-056#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La línea base de cobertura de pruebas está medida y registrada. | T068 |
| `CA-02` | El umbral inicial se registra mediante enmienda cuando exista evidencia. | T068 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T068 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; CA-02 pendiente de enmienda) | Medir la línea base de cobertura |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-08, aprobación en el chat de la sesión ("Si, apruebalos y realizalos"). Sin revisión independiente de seguridad todavía.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
