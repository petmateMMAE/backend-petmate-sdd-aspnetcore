# Spec — pm-108 business-location-update

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`PUT /api/v1/businesses/update-location` cambia las coordenadas del negocio propio.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create

## 3. Origen

- Fuentes: Documento v3: RF-26, RF-31, HU-024, HU-029.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-108#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado actualiza la latitud y la longitud de su negocio. | T126 |
| `CA-02` | Las coordenadas fuera de rango válido se rechazan con 400. | T126 |
| `CA-03` | Solo el dueño del negocio puede cambiarlas. | T126 |
| `CA-04` | Las consultas por cercanía reflejan la ubicación nueva. | T126 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T126 | Nueva | Propuesto | Actualizar la ubicación del negocio |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La geocodificación de la dirección ocurre en la app; el backend solo guarda las coordenadas.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
