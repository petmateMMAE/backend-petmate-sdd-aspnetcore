# Spec — pm-095 pet-qr-regenerate

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/pets/regenerate-qr/{id}` genera un token y un código nuevos y deja de aceptar los anteriores.

## 2. Dependencias

- [pm-094](../pm-094-pet-qr-read/spec.md) — pet-qr-read

## 3. Origen

- Fuentes: Documento v3: RF-41, RNF-03; HU-012.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-095#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Regenerar produce un token y un código corto nuevos y distintos. | T113 |
| `CA-02` | El token y el código anteriores se rechazan de inmediato. | T113 |
| `CA-03` | Solo el dueño puede regenerarlo; una mascota ajena responde 404. | T113 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T113 | Nueva | Propuesto | Regenerar el QR de una mascota propia |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La cancelación de las autorizaciones activas al regenerar (decisión del usuario del 2026-11-09) se especificará con el Spec de autorizaciones de acceso.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
