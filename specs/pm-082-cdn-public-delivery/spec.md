# Spec — pm-082 cdn-public-delivery

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Distribución de imágenes públicas mediante CDN.

## 2. Dependencias

- [pm-073](../pm-073-public-https-endpoint/spec.md) — public-https-endpoint
- [pm-079](../pm-079-image-storage-adapter/spec.md) — image-storage-adapter

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-15 (ver [traceability.md](../traceability.md)).
- Constitución: §8.10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-082#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El CDN distribuye únicamente contenido público. | T094 |
| `CA-02` | El origen privado no queda expuesto por el CDN. | T094 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T094 | Nueva | Propuesto | Configurar el CDN de contenido público |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
