# Spec — pm-080 image-upload-endpoint

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Contrato, límites y validación de subida de imágenes.

## 2. Dependencias

- [pm-045](../pm-045-pet-ownership-bola/spec.md) — pet-ownership-bola
- [pm-079](../pm-079-image-storage-adapter/spec.md) — image-storage-adapter

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-15 (ver [traceability.md](../traceability.md)).
- Constitución: §8.10, §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-080#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El contrato de subida define tipos permitidos y tamaño máximo. | T092 |
| `CA-02` | El contenido del archivo se valida. | T092 |
| `CA-03` | Solo el propietario puede subir imágenes de su mascota. | T092 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T092 | Nueva | Aprobada para iniciar (2026-11-11, aprobacion humana) | Implementar el endpoint de carga de imágenes |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El endpoint no existe en el catálogo; requiere actualizar `docs/api/endpoints.md` antes de implementar.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (dueño del backend), 2026-11-11, aprobación transmitida por Adrián en el chat de la sesión.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
