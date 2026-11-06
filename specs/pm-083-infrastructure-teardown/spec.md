# Spec — pm-083 infrastructure-teardown

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Destrucción ordenada y protegida de la infraestructura.

## 2. Dependencias

- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline
- [pm-067](../pm-067-iac-state-management/spec.md) — iac-state-management
- [pm-068](../pm-068-cloud-secrets-configuration/spec.md) — cloud-secrets-configuration
- [pm-069](../pm-069-atlas-cloud-integration/spec.md) — atlas-cloud-integration
- [pm-070](../pm-070-cloud-redis-auth/spec.md) — cloud-redis-auth
- [pm-071](../pm-071-cloud-redis-cache/spec.md) — cloud-redis-cache
- [pm-072](../pm-072-cloud-network-security/spec.md) — cloud-network-security
- [pm-073](../pm-073-public-https-endpoint/spec.md) — public-https-endpoint
- [pm-074](../pm-074-container-registry/spec.md) — container-registry
- [pm-075](../pm-075-backend-two-instances/spec.md) — backend-two-instances
- [pm-076](../pm-076-load-balancer/spec.md) — load-balancer
- [pm-078](../pm-078-cloud-observability/spec.md) — cloud-observability
- [pm-079](../pm-079-image-storage-adapter/spec.md) — image-storage-adapter
- [pm-082](../pm-082-cdn-public-delivery/spec.md) — cdn-public-delivery

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-10, AC-16 (ver [traceability.md](../traceability.md)).
- Constitución: §8.15.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-083#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La creación y la destrucción de la infraestructura son reproducibles mediante IaC. | T095 |
| `CA-02` | La destrucción requiere confirmación humana. | T095 |
| `CA-03` | El teardown no destruye datos persistentes ni clínicos (MongoDB Atlas y almacenamiento de datos privados) salvo orden expresa y separada. | T095 |
| `CA-04` | El teardown elimina los recursos que generan costo, incluida la observabilidad. | T095 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T095 | Nueva | Propuesto | Definir y probar el procedimiento de teardown |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
