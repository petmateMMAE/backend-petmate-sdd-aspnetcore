# Spec — pm-052 database-indexing-efficiency

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Inventario de índices y evidencia de consultas críticas.

## 2. Dependencias

- [pm-024](../pm-024-users-persistence/spec.md) — users-persistence
- [pm-025](../pm-025-refresh-sessions-persistence/spec.md) — refresh-sessions-persistence
- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-052#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Existe un inventario de índices por colección y su motivo. | T063 |
| `CA-02` | Las consultas críticas tienen evidencia (`explain`) de uso de índice. | T063 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T063 | Nueva | Propuesto | Inventariar índices y evidenciar consultas críticas |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
