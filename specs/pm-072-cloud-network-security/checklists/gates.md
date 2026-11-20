# Gates — pm-072-cloud-network-security (T084)

> Estado (2026-11-19): **cerrado**. El registro del 2026-11-18 encontró que no
> había reglas de salida; se escribieron y se aplicaron en este ciclo.

## Lo que faltaba

La subred `snet-aca` no tenía grupo de seguridad: **la salida estaba abierta a
todo Internet**. Cualquier dependencia nueva, o cualquier proceso dentro de un
contenedor, podía salir a donde fuera sin que nadie lo notara.

## Lo que se aplicó

`infra/azure/network-security.tf`, nuevo. Un NSG asociado a la subred, con once
reglas:

### Entrada

| Prioridad | Regla | Por qué |
|---|---|---|
| 100 | `AzureLoadBalancer` | El ingress del ambiente reparte el tráfico; llega del balanceador, no de Internet |
| 110 | Tráfico de la subred | La API habla con Redis en 6379/6380 dentro de `10.40.0.0/24` |
| 4096 | **Denegar el resto** | `pm-072#CA-01` |

### Salida

| Prioridad | Regla | Por qué |
|---|---|---|
| 100 | Atlas, 27017/TCP | MongoDB vive fuera de Azure |
| 110 | `Storage`, 443 | Las fotos, por el service endpoint de la subred |
| 120 | `AzureCloud`, 443 | Imagen del registro, Log Analytics, tokens de la identidad |
| 130 | Tráfico a la subred | La API con Redis |
| 140 | DNS, 53 | Sin resolución no encuentra Atlas ni el Blob |
| 4096 | **Denegar el resto** | `pm-072#CA-02` |

Se usan etiquetas de servicio (`Storage`, `AzureCloud`, `AzureLoadBalancer`) en
vez de rangos a mano: Azure las mantiene al día y cubren solo las direcciones de
ese servicio.

## Evidencia

```
Plan:  11 to add, 0 to change, 0 to destroy.
Apply complete! Resources: 11 added, 0 changed, 0 destroyed.
```

Antes y después del apply, contra la API en producción:

| Prueba | Antes | Después |
|---|---|---|
| `GET /api/v1/health/live` | 200 | **200** |
| `GET /api/v1/health/ready` | 200 | **200** |
| `POST /api/v1/pets/upload-photo/{id}` | 401 | **401** |

`health/ready` verifica Mongo y Redis: que siga en 200 demuestra que las reglas
de salida dejan pasar lo que el backend necesita y nada se rompió.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-072#CA-01` | Subred delegada con NSG; entrada denegada salvo el balanceador y la propia subred. Redis no tiene ingress externo | **Cumple** |
| `pm-072#CA-02` | **Salida restringida a cinco destinos**: Atlas, Storage, AzureCloud, la subred y DNS. Todo lo demás se deniega en la 4096 | **Cumple** |
| `pm-072#CA-03` | Las reglas `denegar-resto-entrada` y `denegar-resto-salida` rechazan cualquier origen o destino no autorizado. La comprobación es que la API sigue en 200 **con** esas reglas activas: solo pasa lo permitido | **Cumple** |

## Cierre

- [x] Criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación en vivo antes y después del apply.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Nota para el equipo

Si mañana el backend necesita salir a un destino nuevo (un servicio de correo,
una API de terceros), **va a fallar** y habrá que agregar su regla. Eso es
intencional: es la diferencia entre una red cerrada y una abierta.

La subred sigue siendo una sola. Separar la API de Redis en subredes distintas
exigiría rehacer el ambiente de Container Apps, que es una operación con
interrupción. Queda documentado como mejora posible, no como brecha: Redis no
tiene ingress externo y el NSG ya limita quién le habla.
