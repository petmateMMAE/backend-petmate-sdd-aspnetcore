# Spec — pm-048 response-compression

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Compresión de respuestas con evaluación de riesgos.

## 2. Dependencias

- [pm-009](../pm-009-response-wrapper-contract/spec.md) — response-wrapper-contract

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-048#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las respuestas se comprimen cuando el cliente lo solicita. | T059 |
| `CA-02` | Existe una evaluación documentada de riesgos de la compresión sobre HTTPS y, si se justifica, se excluyen respuestas con material sensible. | T059 |
| `CA-03` | El efecto se mide antes y después. | T059 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T059 | Nueva | Propuesto | Implementar y evaluar la compresión HTTP |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
