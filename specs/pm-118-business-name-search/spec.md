# Spec — pm-118 business-name-search

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

La consulta de negocios acepta un texto para filtrar por nombre.

## 2. Dependencias

- [pm-116](../pm-116-business-nearby-query/spec.md) — business-nearby-query

## 3. Origen

- Fuentes: Maqueta `map` (barra de búsqueda); decisión del usuario del 2026-11-09 (no se recorta nada).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-118#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El texto filtra por nombre sin distinguir mayúsculas ni acentos. | T136 |
| `CA-02` | El texto tiene una longitud mínima y máxima y se trata como dato, nunca como expresión. | T136 |
| `CA-03` | Se puede combinar con el tipo y la cercanía. | T136 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T136 | Nueva | Propuesto | Buscar negocios por nombre |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- HU-020 deja la búsqueda por nombre fuera de alcance, pero la maqueta la incluye; prevalece la decisión del usuario.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
