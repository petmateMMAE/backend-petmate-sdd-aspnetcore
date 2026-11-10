# ADR-005 — Balanceo de carga y HTTPS

- **Estado:** Propuesto (pendiente de aprobación humana)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-073, pm-076
- **Constitución:** §8
- **Costo:** clasificación y estimados en el [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito (pm-063 CA-04): cada decisión cita la clasificación vigente.

## Contexto
La API corre con dos o más instancias (requisito de la evaluación) y debe publicarse por HTTPS con certificado válido y redirección de HTTP a HTTPS.

## Decisión
**Ingress administrado de Azure Container Apps** (Envoy): reparte las solicitudes entre las réplicas, termina TLS 1.2/1.3 y redirige el puerto 80 al 443; sin Azure Load Balancer, IP pública ni recursos adicionales. Clasificación: **Incluido** (estudio, fila 2); certificado y dominio por defecto `*.azurecontainerapps.io`: **Incluido** (fila 4).

## Alternativas dentro de Azure
| Alternativa | Clasificación y motivo de descarte |
|---|---|
| Azure Load Balancer (Standard) delante de las instancias | Posible cargo (regla, IP pública y datos procesados) y no termina TLS; innecesario con el ingress |
| Azure Application Gateway | Posible cargo mensual fijo por la puerta de enlace; no se necesita WAF para el alcance |
| Azure Front Door | Posible cargo con base de ~35 USD/mes (estudio del 2026-11-05); sin necesidad de CDN ni WAF global hoy |

## Consecuencias
- `X-Forwarded-For` trae solo la IP más a la derecha: `TRUSTED_PROXIES` debe listar el último salto y `ForwardLimit = 1` (pm-017).
- Un dominio propio con certificado administrado es una mejora posterior y requiere revisar su costo.
