# Spec — pm-062 cloud-cost-feasibility-study

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L0 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Estudio con fuentes oficiales del costo $0 MXN por componente en Azure for Students y del costo total de operación de la arquitectura.

## 2. Dependencias

- Ninguna.

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-01, AC-02 (ver [traceability.md](../traceability.md)).
- Constitución: §8.1, §8.2, §8.5, §8.6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-062#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cada componente (cómputo para dos instancias, Load Balancer, red y salida de datos, DNS y certificados, registro de contenedores, almacenamiento, CDN, Redis Auth, Redis Cache, observabilidad, secretos, estado de IaC, CI/CD y escaneos, tráfico hacia Atlas) tiene fuente oficial con URL y fecha de consulta. | T074 |
| `CA-02` | Cada componente se clasifica como incluido gratuitamente, cubierto por créditos de Azure for Students o con posible cargo adicional, con el importe estimado en MXN. | T074 |
| `CA-03` | El estudio calcula el costo total de operación y el consumo de créditos de la arquitectura completa. | T074 |
| `CA-04` | Si la arquitectura requerida no se puede cubrir sin desembolso adicional, se documenta el conflicto académico y no se reduce ningún requisito (dos instancias y Load Balancer se mantienen). | T074 |
| `CA-05` | El estudio no crea recursos reales ni habilita servicios con posibles cargos. | T074 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T074 | Nueva | En progreso (documentada y verificada; pendiente de aprobación humana) | Elaborar el estudio de costos de Azure for Students |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Azure for Students:** Insumo para todas las decisiones de servicio Azure. Ningún servicio se asume gratuito por pertenecer a Azure for Students.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
