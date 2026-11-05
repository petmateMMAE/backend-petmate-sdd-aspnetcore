# Spec — pm-075 backend-two-instances

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Al menos dos instancias del backend con sondas y sin estado local.

## 2. Dependencias

- [pm-015](../pm-015-api-container-image/spec.md) — api-container-image
- [pm-068](../pm-068-cloud-secrets-configuration/spec.md) — cloud-secrets-configuration
- [pm-069](../pm-069-atlas-cloud-integration/spec.md) — atlas-cloud-integration
- [pm-070](../pm-070-cloud-redis-auth/spec.md) — cloud-redis-auth
- [pm-071](../pm-071-cloud-redis-cache/spec.md) — cloud-redis-cache
- [pm-072](../pm-072-cloud-network-security/spec.md) — cloud-network-security
- [pm-074](../pm-074-container-registry/spec.md) — container-registry

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-03 (ver [traceability.md](../traceability.md)).
- Constitución: §8.3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-075#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Al menos dos instancias del backend se ejecutan simultáneamente. | T087 |
| `CA-02` | La API es stateless: el reparto entre instancias no pierde sesiones. | T087 |
| `CA-03` | Las sondas live y ready están configuradas. | T087 |
| `CA-04` | Ninguna instancia usa disco local como fuente de verdad. | T087 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T087 | Nueva | Propuesto | Desplegar dos instancias del backend |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
