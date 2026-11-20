# Gates — pm-078-cloud-observability (T090)

> Estado (2026-11-19): **cerrado**. Faltaba la alerta de indisponibilidad; se
> creó y se verificó en este ciclo.

## Lo que faltaba

El registro anterior dejó CA-02 en parcial: solo existía la alerta de errores
5xx. Esa alerta tiene un punto ciego — **solo se dispara si la API responde y
responde mal**. Si el servicio se cae del todo deja de haber peticiones, no hay
5xx que contar y nadie se entera. El servicio podía estar caído horas sin aviso.

## La alerta nueva

`azurerm_monitor_metric_alert.api_sin_replicas` en `monitoring.tf`:

| Parámetro | Valor | Por qué |
|---|---|---|
| Métrica | `Replicas`, promedio | Mide si la API está levantada, no si hay tráfico |
| Condición | Menor que 2 | Con `modo_ahorro = false` el mínimo son 2 |
| Ventana | 15 minutos | Evita falsos positivos durante un despliegue, cuando una revisión reemplaza a la otra |
| Severidad | 1 | Más alta que la de 5xx (2): el servicio caído es peor que errores |

Lleva `count = var.modo_ahorro ? 0 : 1`: en modo ahorro la API baja a 0 réplicas
**a propósito**, así que la alerta no debe existir ahí o avisaría todas las
noches.

## Evidencia

```
Plan: 1 to add, 0 to change, 0 to destroy.
Apply complete! Resources: 1 added, 0 changed, 0 destroyed.

$ az monitor metrics alert list --resource-group rg-petmate-prod
alert-petmate-api-5xx            True   severidad 2
alert-petmate-api-sin-replicas   True   severidad 1
```

La API siguió respondiendo 200 durante y después del apply.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-078#CA-01` | Log Analytics `log-petmate-prod` asociado al ambiente, retención de 30 días. Los registros de la API llegan en JSON estructurado y se leen por réplica (usado en `pm-076`) | **Cumple** |
| `pm-078#CA-02` | **Dos alertas activas**: errores (5xx, severidad 2) e indisponibilidad (réplicas < 2, severidad 1). Ambas notifican al action group `ag-petmate-prod` | **Cumple** |
| `pm-078#CA-03` | `RequestLoggingMiddleware` registra solo método, plantilla de ruta, estado y duración. Los registros reales en producción muestran `"Route":"api/v1/health/ready"`: la plantilla, sin identificadores. Los filtros de `Hosting.Diagnostics` y `HttpsPolicy` impiden que `RequestPath` llegue a los registros | **Cumple** |

### Sobre CA-03

La evidencia no es solo la configuración: se leyeron registros reales de
producción con `az containerapp logs show` y se comprobó que el campo `Route`
trae la plantilla (`api/v1/health/ready`), no la URL con identificadores. Los
ámbitos que acompañan cada evento son `ConnectionId` y `CorrelationId`, ninguno
derivado de datos del usuario.

## Cierre

- [x] Los tres criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación contra Azure en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Lo que no tiene

No hay Application Insights ni trazas distribuidas. Para una API de un solo
servicio, los registros estructurados con identificador de correlación cubren la
necesidad; las trazas tienen sentido cuando hay varios servicios encadenados.
Queda como mejora posible, no como brecha.
