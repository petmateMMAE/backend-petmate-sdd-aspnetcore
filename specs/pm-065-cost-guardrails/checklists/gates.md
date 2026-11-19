# Gates — pm-065-cost-guardrails

> Estado (2026-11-19): verificado contra Azure en vivo. El procedimiento que
> faltaba (CA-03) se escribió en este mismo ciclo.

## Evidencia recogida

```
$ az consumption budget list --query "[].{nombre:name,monto:amount,periodo:timeGrain}"
budget-petmate-prod   40.0   Monthly

$ az resource list --resource-group rg-petmate-prod --query "[].{n:name,tags:tags.proyecto}"
los 11 recursos del grupo devuelven la etiqueta "petmate":
id-petmate-api-prod, ag-petmate-prod, stpetmateprodd8u3u, vnet-petmate-prod,
acrpetmateprodd8u3u, log-petmate-prod, cae-petmate-prod, redis-auth,
redis-cache, petmate-api, alert-petmate-api-5xx
```

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-065#CA-01` | El presupuesto `budget-petmate-prod` **existe en la suscripción**, mensual, 40 USD. Queda sin demostrar que llegara un aviso: no se ha alcanzado ningún umbral | **Parcial** |
| `pm-065#CA-02` | **Los 11 recursos del grupo están etiquetados** con `proyecto=petmate`, auditado con `az resource list`. Era la duda del registro anterior | **Cumple** |
| `pm-065#CA-03` | **Procedimiento escrito**: [`docs/infrastructure/agotamiento-de-creditos.md`](../../../docs/infrastructure/agotamiento-de-creditos.md). Define los umbrales de vigilancia, cuatro recortes en orden, qué hacer si la suscripción ya se deshabilitó, cómo reactivar y quién decide cada cosa. **Deja explícito que nunca se baja a una sola instancia**, que es justo lo que el criterio exige | **Cumple** |

## Lo que queda pendiente de CA-01

El presupuesto existe y está activo, pero no se ha cruzado ningún umbral, así
que no hay evidencia de que un aviso llegue de verdad. Se demostrará cuando se
alcance el 50 % o se fuerce una prueba del action group.

No bloquea el cierre: el criterio pide que el presupuesto y las alertas **estén
definidos**, y se verificó que lo están en la suscripción.

## Cierre

- [x] Los tres criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación contra Azure en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Riesgo vigente

Con `modo_ahorro = false` desde el 2026-11-17, la API y Redis quedan encendidos
todo el día: el consumo contra los 40 USD es continuo, no por demanda. Hay que
vigilar el presupuesto hasta la evaluación del 2026-12-02. El procedimiento
nuevo explica qué recortar y en qué orden si se acerca al límite.
