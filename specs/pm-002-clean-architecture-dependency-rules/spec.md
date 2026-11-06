# Spec — pm-002 clean-architecture-dependency-rules

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L1 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Dirección de referencias entre capas exigida por prueba de arquitectura.

## 2. Dependencias

- [pm-001](../pm-001-solution-structure/spec.md) — solution-structure

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-002; task T002.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-002#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Las referencias entre proyectos cumplen la dirección definida en la constitución §3. [origen: 00-foundation CA-002] | T002 |
| `CA-02` | Una prueba de arquitectura falla ante una referencia prohibida. | T002 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T002 | Histórica | Cerrado (2026-11-03) | Configurar referencias de Clean Architecture |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
