# Spec — pm-066 iac-baseline

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Herramienta de IaC elegida por ADR, estructura y flujo plan/apply.

## 2. Dependencias

- [pm-063](../pm-063-cloud-provider-selection/spec.md) — cloud-provider-selection
- [pm-064](../pm-064-environment-strategy/spec.md) — environment-strategy

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-10, AC-13 (ver [traceability.md](../traceability.md)).
- Constitución: §8.15.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-066#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La herramienta de IaC (preferentemente Terraform) se fija mediante ADR; es una decisión de proyecto. | T078 |
| `CA-02` | La estructura separa `plan` y `apply` y no ejecuta cambios reales sin autorización. | T078 |
| `CA-03` | La configuración de IaC no contiene secretos. | T078 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T078 | Nueva | Propuesto | Establecer la línea base de IaC |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
