# Spec — pm-117 business-type-filter

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

La consulta de negocios acepta uno o varios tipos para filtrar el resultado.

## 2. Dependencias

- [pm-116](../pm-116-business-nearby-query/spec.md) — business-nearby-query

## 3. Origen

- Fuentes: Documento v3: RF-21, HU-020.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-117#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El parámetro de tipo acepta uno o varios valores de Veterinaria, Tienda y Estética. | T135 |
| `CA-02` | Un tipo inválido devuelve 400. | T135 |
| `CA-03` | Sin parámetro se devuelven todos los tipos. | T135 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T135 | Nueva | Propuesto | Filtrar los negocios por tipo |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
