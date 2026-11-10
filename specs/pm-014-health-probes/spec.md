# Spec — pm-014 health-probes

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Liveness sin dependencias y readiness con detalle mínimo por dependencia.

## 2. Dependencias

- [pm-012](../pm-012-atlas-connectivity/spec.md) — atlas-connectivity
- [pm-013](../pm-013-redis-dual-connectivity/spec.md) — redis-dual-connectivity

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-007; task T011.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7, §8.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-014#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | `GET /api/v1/health/live` devuelve 200 sin consultar dependencias. | T045 |
| `CA-02` | `GET /api/v1/health/ready` devuelve 200 o 503 con `data.dependencies[{name,status}]` para `mongodb`, `redis-auth` y `redis-cache`. | T045 |
| `CA-03` | Los health checks no exponen hosts, URIs ni secretos. [origen: 00-foundation CA-007] | T045 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T045 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de health probes |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
