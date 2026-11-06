# Spec — pm-013 redis-dual-connectivity

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Dos conexiones Redis independientes (Auth y Cache) con TLS fuera de Development.

## 2. Dependencias

- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets

## 3. Origen

- Fuentes: 00-foundation/task.md T010; docs/infrastructure/docker-redis-atlas.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-12 (ver [traceability.md](../traceability.md)).
- Constitución: §5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-013#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Existen dos conexiones Redis independientes (Auth y Cache) con configuración distinta. | T044 |
| `CA-02` | Fuera de Development la configuración exige TLS (`ssl=True`). | T044 |
| `CA-03` | La conexión es perezosa y se reconecta sola (`AbortOnConnectFail=false`). | T044 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T044 | Retrospectiva | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de conectividad Redis dual |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La seguridad de Redis en Azure se especifica y verifica en pm-070 (Auth) y pm-071 (Cache); este Spec cubre la conectividad de la aplicación.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
