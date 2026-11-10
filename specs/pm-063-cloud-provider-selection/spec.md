# Spec — pm-063 cloud-provider-selection

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L1 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Registrar Azure for Students como proveedor definitivo y seleccionar mediante ADR el servicio de Azure para cada componente.

## 2. Dependencias

- [pm-062](../pm-062-cloud-cost-feasibility-study/spec.md) — cloud-cost-feasibility-study

## 3. Origen

- Fuentes: nuevo; decisión del usuario 2026-11-05.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-01, AC-02 (ver [traceability.md](../traceability.md)).
- Constitución: §8.1, §8.2.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-063#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El proveedor queda registrado como Microsoft Azure con la suscripción Azure for Students (decisión del usuario del 2026-11-05); no se evalúan AWS ni GCP. | T075 |
| `CA-02` | Existe un ADR por componente con alternativas dentro de Azure (cómputo, balanceo, Redis, almacenamiento, CDN, observabilidad, registro de contenedores, secretos). | T075 |
| `CA-03` | Cada servicio seleccionado cita la clasificación de costo de pm-062. | T075 |
| `CA-04` | Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito. | T075 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T075 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Registrar la decisión de proveedor y los ADR de servicios Azure |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Azure for Students:** Los servicios candidatos (por ejemplo Azure Container Apps o App Service para cómputo; Azure Load Balancer, Application Gateway o el ingress del servicio elegido para balanceo; Azure Monitor/Application Insights; Azure Blob Storage; Azure Front Door o Azure CDN) son hipótesis a validar en pm-062, no decisiones.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
- Ratificación de Miguel (usuario), 2026-11-08: aprobación del spec y de la Task en el chat de la sesión (cierre de spec pendiente).
