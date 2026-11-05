# Spec — pm-084 cloud-deployment-acceptance

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Aceptación del sistema desplegado: dos instancias, balanceador, HTTPS y costo.

## 2. Dependencias

- [pm-046](../pm-046-first-milestone-acceptance/spec.md) — first-milestone-acceptance
- [pm-073](../pm-073-public-https-endpoint/spec.md) — public-https-endpoint
- [pm-075](../pm-075-backend-two-instances/spec.md) — backend-two-instances
- [pm-076](../pm-076-load-balancer/spec.md) — load-balancer
- [pm-077](../pm-077-cd-deployment-pipeline/spec.md) — cd-deployment-pipeline
- [pm-078](../pm-078-cloud-observability/spec.md) — cloud-observability
- [pm-083](../pm-083-infrastructure-teardown/spec.md) — infrastructure-teardown

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-01, AC-02, AC-03, AC-04, AC-05, AC-07, AC-08 (ver [traceability.md](../traceability.md)).
- Constitución: §8.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-084#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Dos instancias, Load Balancer y HTTPS están verificados en el entorno desplegado. | T096 |
| `CA-02` | CI, CD y observabilidad están operativos. | T096 |
| `CA-03` | El costo en MXN demuestra $0 de desembolso directo, o el conflicto con los créditos de Azure for Students está documentado sin reducir requisitos. | T096 |
| `CA-04` | El sistema desplegado recibe aprobación humana. | T096 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T096 | Nueva | Propuesto | Ejecutar la aceptación del despliegue cloud |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
