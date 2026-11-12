# Spec — pm-133 consultation-owner-list

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L13 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/consultations/get-all` lista las consultas registradas por veterinarias para una mascota del dueño.

## 2. Dependencias

- [pm-131](../pm-131-consultation-create/spec.md) — consultation-create
- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance

## 3. Origen

- Fuentes: Documento v3: RF-18, HU-017; maqueta `history`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-133#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño lista las consultas de su mascota de la más reciente a la más antigua. | T151 |
| `CA-02` | Cada elemento incluye fecha, veterinaria, motivo y diagnóstico. | T151 |
| `CA-03` | Una mascota ajena o inexistente responde 404 y el listado usa la convención de paginación. | T151 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T151 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Listar las consultas de una mascota propia |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
