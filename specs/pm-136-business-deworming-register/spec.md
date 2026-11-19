# Spec — pm-136 business-deworming-register

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/dewormings/create-by-business` registra una desparasitación en el carnet de una mascota autorizada.

## 2. Dependencias

- [pm-126](../pm-126-scan-access-check/spec.md) — scan-access-check
- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance
- [pm-099](../pm-099-deworming-create-list/spec.md) — deworming-create-list

## 3. Origen

- Fuentes: Documento v3: RF-36, HU-034.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-136#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Solo una veterinaria con autorización vigente la registra; tienda y estética reciben 403. | T154 |
| `CA-02` | El campo «aplicada por» se llena con el nombre del negocio y el registro tiene origen de negocio. | T154 |
| `CA-03` | El dueño no puede modificar ni borrar el registro. | T154 |
| `CA-04` | La próxima fecha, si se envía, es futura. | T154 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T154 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Registrar una desparasitación aplicada por la veterinaria |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
