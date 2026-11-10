# Spec — pm-051 cache-policy-data-classification

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Política de qué datos pueden cachearse y criterios de decisión.

## 2. Dependencias

- [pm-050](../pm-050-distributed-cache-infrastructure/spec.md) — distributed-cache-infrastructure

## 3. Origen

- Fuentes: nuevo (sustituye al provisional `owner-profile-caching`).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §5, §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-051#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los datos se clasifican (público, privado, personal, sensible) antes de aplicar políticas de caché. | T062 |
| `CA-02` | Los datos personales, clínicos y de autenticación no se cachean salvo decisión documentada y aprobada. | T062 |
| `CA-03` | Cada endpoint cacheable tiene su criterio y TTL justificados. | T062 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T062 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir la política de clasificación de datos y caché |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-08, aprobación en el chat de la sesión ("Aprueba esos specs y empieza"). Sin revisión independiente de seguridad todavía.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
