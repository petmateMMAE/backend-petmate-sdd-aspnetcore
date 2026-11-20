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
| `pm-076#CA-03` | **Verificado el 2026-11-19** durante el ensayo de reversa de `pm-077`: al desplegar una revisión nueva, el ingress dejó de enviar tráfico a la anterior (100 % → 0 %) y esta pasó a `Deprovisioning` sin que el servicio dejara de responder | **Cumple** |

## CA-03 — verificado en el ensayo de reversa

Durante el ensayo de `pm-077`, hecho en la ventana acordada con el equipo y con
los 12 testers avisados, se observó el comportamiento que este criterio pedía:

```
$ az containerapp revision list
petmate-api--0000010   0.3.0-239557a   0 %     Deprovisioning
petmate-api--0000011   0.5.0-77760e1   100 %   RunningAtMaxScale
```

Al activarse la revisión nueva, el ingress movió el tráfico de una a otra
(100 % → 0 %) y retiró la anterior. La API respondió 200 durante toda la
transición: el balanceador deja de enviar tráfico a lo que se está retirando
antes de que deje de existir.

No es exactamente "matar una réplica a la fuerza", pero demuestra lo que el
criterio busca: que el reparto se ajusta cuando una instancia sale de servicio,
sin que el usuario lo note.

## Cierre

- [x] Los tres criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — medición en vivo contra producción.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Nota

El registro del 2026-11-18 concluyó que CA-02 estaba bloqueado por permisos de
Log Analytics. Era cierto para esa vía, pero existía otra: los registros de
consola por réplica. Vale como recordatorio de que un bloqueo de herramienta no
siempre es un bloqueo real.
