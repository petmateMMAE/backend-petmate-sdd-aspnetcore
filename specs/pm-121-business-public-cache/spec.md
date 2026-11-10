# Spec — pm-121 business-public-cache

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Las respuestas públicas de negocios se guardan en Redis Cache con un TTL justificado.

## 2. Dependencias

- [pm-119](../pm-119-business-public-profile/spec.md) — business-public-profile
- [pm-050](../pm-050-distributed-cache-infrastructure/spec.md) — distributed-cache-infrastructure
- [pm-051](../pm-051-cache-policy-data-classification/spec.md) — cache-policy-data-classification

## 3. Origen

- Fuentes: Documento v3: RNF-10; Constitución §5.2.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §5.2.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-121#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las respuestas públicas de cercanía y de perfil se cachean con un TTL justificado en el plan. | T139 |
| `CA-02` | La caché se invalida cuando cambian el negocio, sus servicios o su foto. | T139 |
| `CA-03` | pm-051 se amplía: los datos públicos de negocios son cacheables y los datos personales siguen sin serlo. | T139 |
| `CA-04` | Un fallo de la caché no afecta la respuesta. | T139 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T139 | Nueva | Propuesto | Cachear las respuestas públicas de negocios |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
