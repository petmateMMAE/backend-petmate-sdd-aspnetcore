# Gates — pm-071-cloud-redis-cache (T083)

> Estado (2026-11-19): **cerrado con conflicto de costo documentado**, igual que
> [pm-070](../pm-070-cloud-redis-auth/checklists/gates.md) y por la misma razón.

## El conflicto

Azure Cache for Redis cuesta ~16 USD al mes por instancia. Con auth y cache son
~32 USD mensuales contra un presupuesto total de **40 USD**, que ya sostiene la
API encendida 24/7.

La Spec autoriza esta salida: *"Si ningún servicio cumple a costo $0, se
documenta el conflicto sin reducir TLS ni autenticación."* Eso es lo que se hace
aquí, y TLS y autenticación se conservan.

## Lo que sí se cumple

`redis-cache` corre como Container App con la imagen oficial `redis:7.4`.

### CA-01 — TLS, y sin TLS se rechaza

```
exec redis-server /tmp/redis.conf --port 0 --tls-port 6379 \
  --tls-cert-file /tls/tls-crt --tls-key-file /tls/tls-key
```

`--port 0` apaga el puerto sin cifrar: no existe a dónde conectarse en claro.

### CA-02 — Autenticación obligatoria

`requirepass` se escribe en `/tmp/redis.conf` antes de arrancar.

### CA-03 — Red privada

```
redis-cache   external: False   redis-cache.internal.purplefield-....azurecontainerapps.io
```

`external: False` y FQDN `.internal`: no es alcanzable desde Internet. Desde el
2026-11-19 el NSG de la subred (`pm-072`) refuerza el aislamiento.

### CA-04 — Credenciales distintas de las de Redis Auth

```hcl
resource "random_password" "redis" {
  for_each = local.redis_services
  length   = 48
  special  = false
}
```

El `for_each` genera **una contraseña independiente por servicio**: auth y cache
nunca comparten credencial. Cada una llega a su contenedor como secreto propio
(`redis-password`) y se rota con `terraform taint` sobre esa instancia, sin
tocar la otra.

### CA-05 — Ningún secreto en logs

La contraseña se escribe a un archivo con `umask 077` en vez de pasarse por la
línea de comandos. El pipeline corre gitleaks sobre todo el repositorio.

## Degradación segura

La constitución (§5.2) pide que la caché se degrade sin tumbar la API, y la
configuración lo refleja:

```
--save '' --appendonly no --maxmemory 128mb --maxmemory-policy allkeys-lru
```

Sin persistencia y con expulsión LRU: si la caché se llena, descarta lo menos
usado en vez de fallar. Y si `redis-cache` se cae, la API sigue respondiendo
contra la base — a diferencia de `redis-auth`, que falla cerrado a propósito
porque sin él no se pueden verificar las revocaciones de token.

| CA | Resultado |
|---|---|
| `pm-071#CA-01` | **Cumple** — puerto sin cifrar apagado |
| `pm-071#CA-02` | **Cumple** — `requirepass` obligatorio |
| `pm-071#CA-03` | **Cumple** — sin ingress externo, más el NSG |
| `pm-071#CA-04` | **Cumple** — contraseña independiente por `for_each` |
| `pm-071#CA-05` | **Cumple** — fuera de la línea de comandos |
| Título: "administrado" | **No se cumple**, por costo |

## Cierre

- [x] Los cinco criterios de aceptación satisfechos con evidencia reproducible.
- [x] Conflicto de costo documentado, como la Spec autoriza.
- [x] Revisión independiente — verificación contra el despliegue en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.
