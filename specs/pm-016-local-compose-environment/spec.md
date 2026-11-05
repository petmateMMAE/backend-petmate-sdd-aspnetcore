# Spec — pm-016 local-compose-environment

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Compose local con API y dos Redis, red, volumen y sin MongoDB.

## 2. Dependencias

- [pm-013](../pm-013-redis-dual-connectivity/spec.md) — redis-dual-connectivity
- [pm-014](../pm-014-health-probes/spec.md) — health-probes
- [pm-015](../pm-015-api-container-image/spec.md) — api-container-image

## 3. Origen

- Fuentes: 00-foundation/task.md T011; docs/infrastructure/docker-redis-atlas.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §4, §5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-016#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Compose levanta `api`, `redis-auth` y `redis-cache` y no incluye ningún servicio MongoDB. | T011 |
| `CA-02` | Los Redis no publican puertos, exigen contraseña y funcionan con `read_only` y `cap_drop: ALL`; `redis-auth` usa `noeviction` y AOF. | T011 |
| `CA-03` | La API espera a que los Redis estén saludables y reconecta si caen. | T011 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Health, contenedor y ejecución local |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
