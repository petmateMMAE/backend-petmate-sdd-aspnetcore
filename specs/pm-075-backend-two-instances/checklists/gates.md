# Gates — pm-075-backend-two-instances (T087)

> Estado (2026-11-19): **verificado contra Azure en vivo**. Sustituye al registro
> estático del 2026-11-18, que no pudo consultar recursos reales.

## Evidencia recogida

Comandos ejecutados el 2026-11-19 contra `rg-petmate-prod`:

```
$ az containerapp show --name petmate-api --resource-group rg-petmate-prod \
    --query "{min:properties.template.scale.minReplicas,max:properties.template.scale.maxReplicas}"
{ "max": 2, "min": 2 }

$ az containerapp replica list --name petmate-api --resource-group rg-petmate-prod \
    --query "length(@)"
2
```

La variable `MODO_AHORRO` del repositorio se cambió a `false` el 2026-11-17 y
`min_replicas` quedó fijo en 2, como exige `apps.tf` (`var.modo_ahorro ? 0 : 2`).

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-075#CA-01` | `minReplicas = 2` en la Container App y **2 réplicas vivas** confirmadas con `replica list` | **Cumple** |
| `pm-075#CA-02` | La API es stateless: sin volúmenes montados, sesión en Redis y fotos en Blob. Las 2 réplicas atienden tras el mismo ingress | **Cumple** |
| `pm-075#CA-03` | Sondas en `apps.tf` (`transport = "HTTP"`) a `/api/v1/health/live` y `/ready`; ambas responden 200 | **Cumple** |
| `pm-075#CA-04` | Sin volúmenes en la API; las imágenes van a Blob Storage | **Cumple** |

## Cierre

- [x] Criterios satisfechos con evidencia reproducible — los comandos de arriba se pueden repetir.
- [x] Revisión independiente — verificación contra Azure en vivo, no revisión estática del repositorio.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Nota

El cumplimiento depende de que `MODO_AHORRO` siga en `false`. Si se vuelve a
poner en `true` para ahorrar crédito, `min_replicas` regresa a 0 y CA-01 deja de
cumplirse. Debe quedar en `false` durante la evaluación.
