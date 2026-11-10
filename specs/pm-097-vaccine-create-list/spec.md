# Spec — pm-097 vaccine-create-list

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El dueño registra y consulta las vacunas de su mascota con `POST /api/v1/vaccines/create` y `GET /api/v1/vaccines/get-all`.

## 2. Dependencias

- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance

## 3. Origen

- Fuentes: Documento v3: RF-14, HU-013.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-097#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Se registra una vacuna con fecha de aplicación y nombre obligatorios, y lote, aplicada por y próxima dosis opcionales. | T115 |
| `CA-02` | La próxima dosis debe ser posterior a la fecha de aplicación. | T115 |
| `CA-03` | El listado devuelve las vacunas de una mascota propia ordenadas por fecha, con la próxima dosis. | T115 |
| `CA-04` | Una mascota ajena o inexistente responde 404. | T115 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T115 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Registrar y listar las vacunas de una mascota |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
