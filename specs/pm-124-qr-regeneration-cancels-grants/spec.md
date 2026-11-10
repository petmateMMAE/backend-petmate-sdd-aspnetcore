# Spec — pm-124 qr-regeneration-cancels-grants

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Regenerar el QR de una mascota cancela todas sus autorizaciones activas.

## 2. Dependencias

- [pm-123](../pm-123-access-grant-create/spec.md) — access-grant-create
- [pm-095](../pm-095-pet-qr-regenerate/spec.md) — pet-qr-regenerate

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09; documento v3: RF-41.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-124#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Al regenerar el QR se cancelan todas las autorizaciones activas de esa mascota. | T142 |
| `CA-02` | Un escaneo posterior con una autorización anterior se rechaza. | T142 |
| `CA-03` | La operación es atómica: no queda una autorización viva con un QR nuevo. | T142 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T142 | Nueva | Propuesto | Cancelar las autorizaciones al regenerar el QR |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Completa pm-095, cuyo Spec no cubre las autorizaciones.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
