# Decisiones de arquitectura (ADR)

> Cada decisión de servicio Azure registra alternativas **dentro de Azure** y cita la clasificación de costo del [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ninguna se toma por haber tenido históricamente un nivel gratuito (pm-063 CA-04).

| ADR | Tema | Componentes | Estado |
|---|---|---|---|
| [ADR-001](../../specs/_historico/00-foundation/decisions/ADR-001-mediator-propio.md) | Mediator propio | Aplicación | Histórico |
| [ADR-002](../../specs/_historico/00-foundation/decisions/ADR-002-openapi-scalar.md) | OpenAPI y Scalar | API | Histórico |
| [ADR-003](ADR-003-azure-container-apps.md) | Servicios de Azure del primer despliegue | **Cómputo**, **Redis**, red, registro, secretos, observabilidad, Atlas | Propuesto |
| [ADR-004](ADR-004-terraform.md) | Terraform como IaC | Infraestructura como código | Propuesto |
| [ADR-005](ADR-005-balanceo-y-https.md) | Balanceo y HTTPS | **Balanceo** | Propuesto |
| [ADR-006](ADR-006-almacenamiento-de-imagenes.md) | Almacenamiento de imágenes | **Almacenamiento** (diferido) | Propuesto |
| [ADR-007](ADR-007-cdn.md) | CDN | **CDN** (diferido) | Propuesto |
| [ADR-008](ADR-008-observabilidad.md) | Observabilidad | **Observabilidad** | Propuesto |
| [ADR-009](ADR-009-registro-de-contenedores.md) | Registro de contenedores | **Registro de contenedores** | Propuesto |
| [ADR-010](ADR-010-secretos.md) | Gestión de secretos | **Secretos** | Propuesto |

Cobertura de pm-063 CA-02: cómputo y Redis (ADR-003, con App Service, AKS y Azure Managed Redis como alternativas descartadas), balanceo (ADR-005), almacenamiento (ADR-006), CDN (ADR-007), observabilidad (ADR-008), registro (ADR-009) y secretos (ADR-010).
