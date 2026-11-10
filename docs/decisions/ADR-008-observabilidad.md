# ADR-008 — Observabilidad

- **Estado:** Propuesto (pendiente de aprobación humana)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-054, pm-078
- **Constitución:** §8.14
- **Costo:** clasificación y estimados en el [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito (pm-063 CA-04): cada decisión cita la clasificación vigente.

## Contexto
Hace falta ver logs y recibir alertas (5xx, presupuesto) sin registrar datos personales ni secretos, con el menor costo posible.

## Decisión
**Log Analytics** (logs de consola y del sistema de Container Apps) con **tope diario de ingesta de 0.15 GB**, retención mínima y **alerta de errores 5xx** por correo; Cost Management para presupuesto. Clasificación: **Crédito** (estudio, fila 11, ~1 USD/mes); no se supone una franja gratuita de ingesta.

## Alternativas dentro de Azure
| Alternativa | Clasificación y motivo de descarte |
|---|---|
| Application Insights (basado en el mismo espacio) | Misma facturación por GB; añade telemetría que aún no se necesita ni se ha revisado para datos personales |
| Solo métricas de Azure Monitor | Sin consulta de logs de la aplicación para diagnóstico |
| Event Hubs / exportación | Posible cargo adicional sin necesidad actual |

## Consecuencias
- El logging estructurado sin datos personales (pm-054) queda pendiente; hoy no se registran eventos de seguridad (hallazgo I2).
- El tope diario puede descartar logs si se supera: aceptable para la demo.
