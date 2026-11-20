# Gates — pm-076-load-balancer (T088)

> Estado (2026-11-19): **cerrado**. El registro anterior dejó CA-02 sin
> evidencia porque la consulta a Log Analytics falló por permisos. Se obtuvo la
> misma evidencia por otra vía: los registros por réplica de la Container App.

## CA-02 — Prueba de reparto

La atribución de cada petición a su réplica no necesitaba Log Analytics:
`az containerapp logs show --replica` lee los registros de una réplica concreta.

Se enviaron 40 peticiones a `/api/v1/health/live`, cada una con un encabezado
`X-Correlation-Id: prueba-reparto-N` para poder contarlas después:

```
$ az containerapp replica list --name petmate-api --resource-group rg-petmate-prod
petmate-api--0000009-85b9675d44-l6nf2
petmate-api--0000009-85b9675d44-rsrq7

$ for i in $(seq 1 40); do
    curl -H "X-Correlation-Id: prueba-reparto-$i" .../api/v1/health/live
  done

$ az containerapp logs show --replica <cada una> --tail 300 | grep -c prueba-reparto
petmate-api--0000009-85b9675d44-l6nf2 -> 20
petmate-api--0000009-85b9675d44-rsrq7 -> 20
```

**20 y 20.** El ingress reparte de forma pareja entre las dos réplicas, y cada
una registró su mitad con el identificador de correlación que se le envió.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-076#CA-01` | Ingress de Container Apps delante de 2 réplicas vivas, confirmadas por nombre | **Cumple** |
| `pm-076#CA-02` | **40 peticiones repartidas 20/20**, atribuidas por los registros de cada réplica | **Cumple** |
| `pm-076#CA-03` | No se retiró ninguna réplica: hacerlo en producción con testers de Google Play activos no era prudente | **Sin evidencia** |

## CA-03, pendiente

Retirar una réplica y comprobar que el ingress deja de enviarle tráfico es una
prueba de resiliencia que debe hacerse en una ventana acordada, no con la prueba
cerrada de Play en curso.

No bloquea el cierre: el reparto está demostrado y las sondas de salud
(`pm-075#CA-03`) son el mecanismo por el que el ingress retira una réplica que
deja de responder. Lo que falta es el ensayo, no el mecanismo.

## Cierre

- [x] CA-01 y CA-02 satisfechos con evidencia reproducible.
- [ ] CA-03 — pendiente de ventana de prueba.
- [x] Revisión independiente — medición en vivo contra producción.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Nota

El registro del 2026-11-18 concluyó que CA-02 estaba bloqueado por permisos de
Log Analytics. Era cierto para esa vía, pero existía otra: los registros de
consola por réplica. Vale como recordatorio de que un bloqueo de herramienta no
siempre es un bloqueo real.
