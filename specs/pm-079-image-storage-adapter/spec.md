# Spec — pm-079 image-storage-adapter

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Almacenamiento de imágenes privado por defecto.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence
- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline

## 3. Origen

- Fuentes: nuevo; 03-pet-management/spec.md (fotos pospuestas).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-15 (ver [traceability.md](../traceability.md)).
- Constitución: §8.10.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-079#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El almacenamiento de imágenes en Azure Blob Storage es privado por defecto. | T091 |
| `CA-02` | Ninguna imagen se guarda en el filesystem del contenedor. | T091 |
| `CA-03` | El acceso se abstrae mediante un adaptador. | T091 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T091 | Nueva | Propuesto | Implementar el adaptador de almacenamiento de imágenes |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
