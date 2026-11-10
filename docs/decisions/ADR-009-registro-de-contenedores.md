# ADR-009 — Registro de contenedores

- **Estado:** Propuesto (pendiente de aprobación humana)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-074
- **Constitución:** §8.12
- **Costo:** clasificación y estimados en el [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito (pm-063 CA-04): cada decisión cita la clasificación vigente.

## Contexto
Las imágenes se publican con etiquetas inmutables `X.Y.Z-<sha7>` y deben ser trazables a la versión y al despliegue.

## Decisión
**Azure Container Registry Basic**, sin usuario administrador; las réplicas extraen con identidad administrada (rol `AcrPull`); las etiquetas se bloquean tras publicarse. Clasificación: **Crédito, a confirmar** (estudio, fila 5, ~5 USD/mes; ACR figura entre los servicios gratuitos 12 meses de la oferta de estudiantes).

## Alternativas dentro de Azure
| Alternativa | Clasificación y motivo de descarte |
|---|---|
| ACR Standard / Premium | Mayor costo y funciones (replicación geográfica, redes privadas) que no se necesitan |
| Usar un registro externo (GitHub Container Registry) | Fuera de Azure: la política del proyecto exige servicios de Azure |

## Consecuencias
La imagen final es `aspnet:10.0-noble-chiseled`, escaneada con Trivy antes de publicarse (0 HIGH/CRITICAL, sin ignorar hallazgos sin parche).
