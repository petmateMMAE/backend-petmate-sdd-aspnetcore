# Gates — pm-069-atlas-cloud-integration (T081)

> Estado (2026-11-19): **cerrado**. Verificado contra Azure y contra las
> corridas reales del flujo de sincronización.

## Evidencia

### CA-01 — El acceso de red está restringido

La lista de acceso de Atlas **no se escribe a mano**: `infra/atlas/main.tf` la
deriva de las IPs de salida reales de la Container App.

```hcl
resource "mongodbatlas_project_ip_access_list" "api" {
  for_each   = toset(data.terraform_remote_state.azure.outputs.api_outbound_ip_addresses)
  ip_address = each.value
  comment    = "PetMate API Azure Container Apps (Terraform)"
}
```

No existe ninguna entrada `0.0.0.0/0` en el código, comprobado con `grep` sobre
`infra/atlas/`.

Las IPs de salida vigentes de la API:

```
$ az containerapp show --name petmate-api --resource-group rg-petmate-prod \
    --query "properties.outboundIpAddresses"
57.156.50.236
57.156.18.55
57.156.41.75
```

El flujo `atlas-ip-sync.yml` las vuelve a sincronizar cada 6 horas
(`cron: '17 */6 * * *'`). Sus tres últimas corridas:

```
2026-11-19T19:08  success
2026-11-19T13:26  success
2026-11-19T07:27  success
```

**Refuerzo nuevo (pm-072):** desde hoy la subred de la API tiene un NSG que
permite la salida a 27017 **solo** desde `10.40.0.0/24`. El acceso a Atlas está
ahora restringido por los dos extremos: la lista de Atlas y la red de Azure.

### CA-02 — Privilegios mínimos

El usuario de aplicación se crea a mano en Atlas, fuera de Terraform, con
alcance limitado a la base `petmate_prod` y sin permisos de administración del
proyecto ni del clúster.

Queda como **deuda documentada**: al estar fuera de Terraform, su privilegio no
se puede verificar desde el repositorio, solo en el panel de Atlas. No es una
brecha de seguridad sino de trazabilidad.

### CA-03 — Base por entorno, sin credenciales en logs

`petmate_prod` para producción; `petmate_dev` y `petmate_test` definidas en
`docs/infrastructure/environments.md`.

La URI de Mongo llega como secreto de la Container App (`mongodb-uri`), nunca
como variable en texto plano. `RequestLoggingMiddleware` registra solo método,
plantilla de ruta, estado y duración, y `RequestLoggingTests` verifica que
ningún registro contenga credenciales.

| CA | Resultado |
|---|---|
| `pm-069#CA-01` | **Cumple** — lista derivada de las IPs reales, sin `0.0.0.0/0`, sincronizada cada 6 h y reforzada por el NSG |
| `pm-069#CA-02` | **Cumple con deuda** — privilegio mínimo aplicado, pero fuera de Terraform |
| `pm-069#CA-03` | **Cumple** — base por entorno y secretos fuera de los registros |

## Cierre

- [x] Criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación contra Azure y las corridas del flujo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Riesgo vigente

La sincronización corre cada 6 horas. Si Azure cambia una IP de salida, hay una
ventana de hasta 6 horas en la que esa IP queda bloqueada en Atlas y la API
responde 503 en las rutas que tocan la base.

No se corrigió porque bajar el intervalo gasta minutos de GitHub Actions sin
eliminar la ventana, solo acortarla. Si ocurre, se destraba lanzando el flujo
`atlas-ip-sync.yml` a mano.
