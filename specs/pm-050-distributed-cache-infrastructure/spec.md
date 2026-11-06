# Spec — pm-050 distributed-cache-infrastructure

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Adaptador de caché sobre Redis Cache, claves, TTL y política de falla.

## 2. Dependencias

- [pm-013](../pm-013-redis-dual-connectivity/spec.md) — redis-dual-connectivity

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-12 (ver [traceability.md](../traceability.md)).
- Constitución: §5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-050#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El adaptador de caché usa exclusivamente Redis Cache y nunca Redis Auth. | T061 |
| `CA-02` | Existen convención de claves, TTL y política de falla definidas. | T061 |
| `CA-03` | Una falla de la caché no afecta la corrección funcional. | T061 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T061 | Nueva | Propuesto | Implementar el adaptador de caché distribuida sobre Redis Cache |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
