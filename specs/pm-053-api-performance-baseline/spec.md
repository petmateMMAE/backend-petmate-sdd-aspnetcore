# Spec — pm-053 api-performance-baseline

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Objetivos de latencia medibles y prueba de carga sin costo.

## 2. Dependencias

- [pm-046](../pm-046-first-milestone-acceptance/spec.md) — first-milestone-acceptance

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-053#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los objetivos de latencia (p. ej. p95) están definidos y son medibles. | T064 |
| `CA-02` | Una prueba de carga con herramienta de costo $0 produce resultados reproducibles. | T064 |
| `CA-03` | Los resultados se registran sin PII. | T064 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T064 | Nueva | En progreso (implementada y verificada; pendiente de aprobación humana) | Medir la línea base de rendimiento |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-08, aprobación en el chat de la sesión ("Aprueba esos specs y empieza"). Sin revisión independiente de seguridad todavía.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
