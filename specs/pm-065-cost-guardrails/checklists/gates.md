# Gates — pm-065-cost-guardrails

> Estado (2026-11-19): verificado contra Azure en vivo. Sustituye al registro
> estático del 2026-11-18. **CA-03 sigue sin cumplirse.**

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
| `pm-065#CA-03` | Sigue sin existir un procedimiento propio ante agotamiento de créditos: solo la frase de `azure-deployment.md` | **No cumple** |

## Razones para no cerrar

- **CA-03** exige un procedimiento escrito: qué hacer si se agota el crédito,
  cómo evitar quedar con una sola instancia y quién decide. No existe.
- **CA-01** quedará demostrado cuando se cruce un umbral y llegue el aviso, o
  se fuerce una prueba del action group.
- El presupuesto está en USD; `pm-084#CA-03` pide demostrar el costo en MXN.

## Cierre

- [x] CA-02 satisfecho con evidencia reproducible.
- [ ] CA-01 — pendiente de un aviso real.
- [ ] CA-03 — falta escribir el procedimiento.
- [ ] Aprobación humana: **no se firma** con CA-03 incumplido.

## Riesgo

Con `modo_ahorro = false` desde el 2026-11-17, la API y Redis quedan encendidos
todo el día. El consumo contra los 40 USD es ahora continuo, no por demanda.
Conviene vigilar el presupuesto hasta la evaluación del 2026-12-02.
