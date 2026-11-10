# Spec — pm-116 business-nearby-query

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/businesses/get-nearby` devuelve los negocios activos cercanos a unas coordenadas, también para quien no tiene cuenta.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create
- [pm-106](../pm-106-business-schedule-model/spec.md) — business-schedule-model
- [pm-047](../pm-047-pagination-convention/spec.md) — pagination-convention

## 3. Origen

- Fuentes: Documento v3: RF-20, RF-22, HU-019, HU-021, RNF-10; maqueta `map`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-116#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La consulta devuelve los negocios activos dentro de un radio (5 km por defecto, con máximo) ordenados por distancia. | T134 |
| `CA-02` | Cada elemento incluye foto, nombre, tipo, si está abierto ahora, el horario de hoy y la distancia. | T134 |
| `CA-03` | Es accesible sin cuenta (AllowAnonymous) y usa la convención de paginación. | T134 |
| `CA-04` | El tiempo de respuesta con la línea base se mide y se registra frente al objetivo de 2 segundos. | T134 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T134 | Nueva | Propuesto | Consultar los negocios cercanos de forma pública |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Sirve también al modo demo del Visitante (afiliados reales).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
