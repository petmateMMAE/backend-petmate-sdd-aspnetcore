# ADR-007 — Distribución de contenido (CDN)

- **Estado:** Propuesto (pendiente de aprobación humana)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-082
- **Constitución:** §8
- **Costo:** clasificación y estimados en el [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito (pm-063 CA-04): cada decisión cita la clasificación vigente.

## Contexto
La entrega pública de imágenes (pm-082) podría beneficiarse de una CDN, pero hoy no existen endpoints de imágenes y el presupuesto es el crédito de Azure for Students.

## Decisión
**Diferida.** Cuando haya contenido público, evaluar **Azure CDN (Microsoft)** o **Azure Front Door** con el precio y el crédito vigentes. Clasificación: **Posible cargo — diferido** (estudio, fila 8); Front Door tiene base de ~35 USD/mes, lo que excede el presupuesto de la demo. No se crea ningún recurso.

## Alternativas dentro de Azure
| Alternativa | Clasificación y motivo |
|---|---|
| Azure CDN estándar | Se evalúa primero por su modelo de cobro por uso; confirmar disponibilidad vigente antes de usarlo |
| Azure Front Door | Base mensual alta; solo si se necesita balanceo global o WAF |
| Sin CDN (Blob con URL temporal) | $0 adicional; válido mientras el tráfico sea bajo |

## Consecuencias
Si no cabe en el crédito, se documenta como brecha y se sirve desde Blob con URLs temporales (pm-081).
