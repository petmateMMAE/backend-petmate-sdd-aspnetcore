# Spec — pm-104 pet-delete-cascade

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Eliminar una mascota borra también todo su carnet y deja de funcionar su QR.

## 2. Dependencias

- [pm-044](../pm-044-pet-soft-delete/spec.md) — pet-soft-delete
- [pm-093](../pm-093-pet-qr-token/spec.md) — pet-qr-token
- [pm-097](../pm-097-vaccine-create-list/spec.md) — vaccine-create-list
- [pm-099](../pm-099-deworming-create-list/spec.md) — deworming-create-list
- [pm-101](../pm-101-weight-create-list/spec.md) — weight-create-list
- [pm-102](../pm-102-allergy-create-list/spec.md) — allergy-create-list

## 3. Origen

- Fuentes: Documento v3: RF-12, HU-010.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-104#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Eliminar una mascota borra también sus vacunas, desparasitaciones, pesos y alergias. | T122 |
| `CA-02` | El QR de la mascota eliminada deja de funcionar. | T122 |
| `CA-03` | Una falla a mitad del borrado no deja registros huérfanos (la estrategia se fija en `plan.md`). | T122 |
| `CA-04` | Una prueba falla si una colección con referencia a la mascota no participa en la cascada. | T122 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T122 | Nueva | Propuesto | Eliminar el carnet y el QR al eliminar una mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Las colecciones que se agreguen al carnet después deben incorporarse a la cascada.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
