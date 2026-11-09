# Spec — pm-111 business-photo-public-storage

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

La foto del negocio se guarda en Azure Blob Storage en un contenedor de acceso público separado del de las fotos privadas.

## 2. Dependencias

- [pm-105](../pm-105-business-persistence-model/spec.md) — business-persistence-model
- [pm-079](../pm-079-image-storage-adapter/spec.md) — image-storage-adapter

## 3. Origen

- Fuentes: Documento v3: RF-23, RF-25, RNF-12; decisión del usuario del 2026-11-09 (Blob aprobado; fotos de negocios públicas).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §8.10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-111#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La foto del negocio se guarda en un contenedor o cuenta de acceso público distinto del que guarda las fotos de mascotas. | T129 |
| `CA-02` | La foto pesa como máximo 1 MB y debe ser una imagen de tipo permitido. | T129 |
| `CA-03` | Solo el afiliado sube, reemplaza o elimina la foto de su negocio. | T129 |
| `CA-04` | ADR-006 se enmienda para registrar el contenedor público y su costo verificado. | T129 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T129 | Nueva | Propuesto | Guardar la foto pública del negocio en Blob |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Antes de crear cualquier recurso se verifica el costo vigente y se necesita autorización expresa (`AGENTS.md` §4 y Constitución §8.6). El CDN no está aprobado.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
