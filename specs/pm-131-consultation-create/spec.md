# Spec — pm-131 consultation-create

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/consultations/create` registra una consulta en el carnet de una mascota autorizada.

## 2. Dependencias

- [pm-126](../pm-126-scan-access-check/spec.md) — scan-access-check
- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create
- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance

## 3. Origen

- Fuentes: Documento v3: RF-35, HU-033; maqueta `consultForm`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-131#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Solo una veterinaria con autorización vigente registra una consulta; tienda y estética reciben 403. | T149 |
| `CA-02` | Motivo y diagnóstico son obligatorios y el peso es opcional. | T149 |
| `CA-03` | La consulta queda en el carnet con el negocio como origen y es de solo lectura para el dueño. | T149 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T149 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Registrar una consulta veterinaria |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Si el peso de la consulta se suma al historial de peso queda por decidir (nota de HU-015).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
