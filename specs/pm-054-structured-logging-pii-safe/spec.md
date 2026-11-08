# Spec — pm-054 structured-logging-pii-safe

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Logging estructurado con correlación y sin datos personales.

## 2. Dependencias

- [pm-010](../pm-010-global-exception-handling/spec.md) — global-exception-handling

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-08 (ver [traceability.md](../traceability.md)).
- Constitución: §6, §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-054#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los logs son estructurados y llevan un identificador de correlación. | T065 |
| `CA-02` | Los logs no contienen credenciales, tokens ni PII. | T065 |
| `CA-03` | Existen pruebas que verifican la ausencia de secretos en logs. | T065 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T065 | Nueva | En progreso (implementada y verificada; pendiente de aprobación humana) | Implementar logging estructurado sin PII |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-08, aprobación en el chat de la sesión ("Si, apruebalos y realizalos"). Sin revisión independiente de seguridad todavía.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
