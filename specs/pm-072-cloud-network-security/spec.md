# Spec — pm-072 cloud-network-security

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Red, reglas de entrada y salida y segmentación.

## 2. Dependencias

- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-12 (ver [traceability.md](../traceability.md)).
- Constitución: §8.8.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-072#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La red está segmentada y los servicios de datos no son accesibles desde Internet abierta. | T084 |
| `CA-02` | Las reglas de entrada y salida son las mínimas necesarias. | T084 |
| `CA-03` | Existen pruebas de acceso denegado desde orígenes no autorizados. | T084 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T084 | Nueva | Propuesto | Definir red y seguridad de red |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
