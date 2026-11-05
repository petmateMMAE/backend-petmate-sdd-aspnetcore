# Spec — pm-069 atlas-cloud-integration

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Acceso de red y usuarios de Atlas para los entornos cloud.

## 2. Dependencias

- [pm-012](../pm-012-atlas-connectivity/spec.md) — atlas-connectivity
- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline
- [pm-068](../pm-068-cloud-secrets-configuration/spec.md) — cloud-secrets-configuration
- [pm-072](../pm-072-cloud-network-security/spec.md) — cloud-network-security

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-11 (ver [traceability.md](../traceability.md)).
- Constitución: §4, §8.8.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-069#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El acceso de red a Atlas desde el entorno Azure está restringido a los orígenes autorizados. | T081 |
| `CA-02` | Los usuarios de aplicación tienen privilegios mínimos. | T081 |
| `CA-03` | Cada entorno usa una base distinta y las credenciales no aparecen en logs. | T081 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T081 | Nueva | Propuesto | Integrar MongoDB Atlas con el entorno Azure |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
