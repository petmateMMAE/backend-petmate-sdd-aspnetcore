# Spec — pm-120 public-read-rate-limit

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Las consultas públicas de negocios tienen un límite por IP compartido entre réplicas.

## 2. Dependencias

- [pm-116](../pm-116-business-nearby-query/spec.md) — business-nearby-query
- [pm-036](../pm-036-rate-limiting/spec.md) — rate-limiting

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09 (el mapa es público); Constitución §6.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-120#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las consultas públicas de negocios devuelven 429 con `Retry-After` al exceder su límite por IP. | T138 |
| `CA-02` | El contador se comparte entre réplicas mediante Redis, como en pm-036#CA-05. | T138 |
| `CA-03` | El límite figura en la matriz de límites de `docs/api/endpoints.md`. | T138 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T138 | Nueva | Propuesto | Limitar por IP las lecturas públicas de negocios |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
