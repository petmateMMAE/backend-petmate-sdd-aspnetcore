# Spec — pm-081 private-image-access

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

URLs temporales para imágenes privadas.

## 2. Dependencias

- [pm-079](../pm-079-image-storage-adapter/spec.md) — image-storage-adapter
- [pm-080](../pm-080-image-upload-endpoint/spec.md) — image-upload-endpoint

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-15 (ver [traceability.md](../traceability.md)).
- Constitución: §8.10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-081#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las imágenes privadas se acceden mediante URLs temporales. | T093 |
| `CA-02` | La autorización se verifica antes de emitir la URL. | T093 |
| `CA-03` | No existen URLs públicas permanentes para información sensible. | T093 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T093 | Nueva | Propuesto | Implementar el acceso privado a imágenes |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
