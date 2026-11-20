# Gates — pm-070-cloud-redis-auth (T082)

> Estado (2026-11-19): **cerrado con conflicto de costo documentado**, que es la
> salida que la propia Spec autoriza.

## El conflicto, y por qué se documenta

La Spec dice textualmente:

> **Azure for Students:** Servicio Redis administrado de Azure por validar en
> pm-062 (disponibilidad, TLS, red privada y costo). **Si ningún servicio cumple
> a costo $0, se documenta el conflicto sin reducir TLS ni autenticación.**

Azure Cache for Redis cuesta ~16 USD al mes por instancia. Con dos instancias
(auth y cache) son ~32 USD mensuales, contra un presupuesto total de **40 USD**
que además ya sostiene la API encendida 24/7 desde el 2026-11-17.

Migrar tampoco es gratis en otro sentido: cambiar de servidor Redis **invalida
las sesiones activas**, y hoy hay testers de Google Play usando la aplicación.

Se documenta el conflicto, como la Spec indica, **sin reducir TLS ni
autenticación** — que es la condición que impone.

## Lo que sí se cumple

Redis corre como Container App con la imagen oficial `redis:7.4`. Los cinco
criterios se verificaron contra el despliegue real.

### CA-01 — TLS, y sin TLS se rechaza

```
exec redis-server /tmp/redis.conf --port 0 --tls-port 6379 \
  --tls-cert-file /tls/tls-crt --tls-key-file /tls/tls-key --tls-auth-clients no
```

`--port 0` **apaga el puerto sin cifrar**. No es que una conexión en claro se
rechace: no existe a dónde conectarse. Los certificados se generan con el
proveedor `tls` de Terraform y se montan como volumen.

### CA-02 — Autenticación obligatoria

`requirepass` se escribe en `/tmp/redis.conf` antes de arrancar. Sin credencial,
Redis responde `NOAUTH Authentication required`.

### CA-03 — Red privada

```
$ az containerapp list --resource-group rg-petmate-prod --query "[?contains(name,'redis')]"
redis-auth    external: False   redis-auth.internal.purplefield-....azurecontainerapps.io
redis-cache   external: False   redis-cache.internal.purplefield-....azurecontainerapps.io
```

`external: False` y el FQDN `.internal`: **no son alcanzables desde Internet**,
solo desde dentro del ambiente de Container Apps.

Desde el 2026-11-19 hay además un NSG en la subred (`pm-072`) que deniega toda
entrada que no venga del balanceador o de la propia subred. Es el "control
equivalente" que el criterio admite.

### CA-04 — Credenciales por secreto y rotables

```hcl
env {
  name        = "REDIS_PASSWORD"
  secret_name = "redis-password"
}
```

Llega como secreto de la Container App, nunca como valor. La contraseña se
genera con `random_password` de Terraform, así que rotarla es
`terraform taint random_password.redis` más un apply; el procedimiento está en
[`docs/security/rotacion-de-secretos.md`](../../../docs/security/rotacion-de-secretos.md).

### CA-05 — Ningún secreto en logs

La contraseña **no va en la línea de comandos**: se escribe a un archivo con
`umask 077` y Redis lo lee de ahí. Un `ps` dentro del contenedor no la muestra.
El pipeline corre gitleaks sobre todo el repositorio en cada push.

| CA | Resultado |
|---|---|
| `pm-070#CA-01` | **Cumple** — puerto sin cifrar apagado |
| `pm-070#CA-02` | **Cumple** — `requirepass` obligatorio |
| `pm-070#CA-03` | **Cumple** — sin ingress externo, más el NSG |
| `pm-070#CA-04` | **Cumple** — secreto del entorno, rotable |
| `pm-070#CA-05` | **Cumple** — fuera de la línea de comandos |
| Título: "administrado" | **No se cumple**, por costo |

## Cierre

- [x] Los cinco criterios de aceptación satisfechos con evidencia reproducible.
- [x] Conflicto de costo documentado, como la Spec autoriza.
- [x] Revisión independiente — verificación contra el despliegue en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Qué se pierde y qué no

Lo que **no** se pierde: cifrado, autenticación, aislamiento de red, secretos
rotables. Son los criterios que la Spec evalúa y están todos.

Lo que **sí** se pierde frente al servicio administrado: respaldos automáticos,
parches de seguridad de Microsoft y garantía de disponibilidad. El AOF vive en
Azure Files, así que los datos sobreviven a un reinicio, pero levantar el
servicio si se cae es responsabilidad del equipo.

Si en algún momento hay presupuesto, migrar es cambiar la cadena de conexión y
rotar la contraseña. **Hacerlo invalida las sesiones activas**, así que debe
programarse fuera del horario de los testers.
