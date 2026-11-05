# Spec — pm-068 cloud-secrets-configuration

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Inyección de secretos y configuración sin `.env` en la nube.

## 2. Dependencias

- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline
- [pm-067](../pm-067-iac-state-management/spec.md) — iac-state-management

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-11 (ver [traceability.md](../traceability.md)).
- Constitución: §8.11, §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-068#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los secretos se inyectan como configuración segura del entorno Azure. | T080 |
| `CA-02` | `.env` nunca se despliega. | T080 |
| `CA-03` | Existe un procedimiento de rotación documentado. | T080 |
| `CA-04` | Un almacén de secretos (Azure Key Vault) solo se usa si su modalidad no genera cargos. | T080 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T080 | Nueva | Propuesto | Configurar secretos y configuración en Azure |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
