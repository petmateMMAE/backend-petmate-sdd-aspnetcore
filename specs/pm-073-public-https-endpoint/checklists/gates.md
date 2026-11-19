# Gates — pm-073-public-https-endpoint (T085)

> Estado (2026-11-19): verificado contra Azure en vivo y **cerrado**: el HSTS
> que faltaba se implementó en este mismo ciclo.

## Evidencia recogida

```
$ curl -s -D- -o /dev/null https://petmate-api.purplefield-b84659a6.chilecentral.azurecontainerapps.io/api/v1/health/live
HTTP/1.1 200 OK

$ curl -s -D- -o /dev/null http://petmate-api.purplefield-b84659a6.chilecentral.azurecontainerapps.io/api/v1/health/live
HTTP/1.1 301 Moved Permanently
location: https://petmate-api.purplefield-b84659a6.chilecentral.azurecontainerapps.io/api/v1/health/live

$ az containerapp show ... --query "properties.configuration.ingress.allowInsecure"
false
```

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-073#CA-01` | Certificado administrado de Azure válido: la petición HTTPS responde 200 sin advertencia de certificado | **Cumple** |
| `pm-073#CA-02` | HTTP responde **301** hacia HTTPS, comprobado en vivo. `allowInsecure: false` en el ingress | **Cumple** |
| `pm-073#CA-03` | **HSTS implementado** en este ciclo: `app.UseHsts()` fuera de Development y `AddHsts` con `max-age` de un año. Cubierto por `HstsHeaderTests` (2 pruebas: lo declara en Production, no lo declara en Development) | **Cumple** |
| `pm-073#CA-04` | `TRUSTED_PROXIES` está en `apps.tf` y el middleware solo se activa si la variable tiene valor; sin ella, `StartupDiagnostics` avisa al arrancar en producción | **Parcial** |

## Decisiones del HSTS

- **`max-age` de un año.** El valor por omisión de ASP.NET son 30 días, que
  reabre cada mes la ventana de la primera petición en claro.
- **Sin `includeSubDomains`.** El host es un subdominio compartido de
  `*.azurecontainerapps.io`: marcar sus hermanos como solo-HTTPS afectaría a
  aplicaciones de terceros. Se puede activar con dominio propio.
- **Sin `preload`.** Inscribir el dominio en la lista precargada de los
  navegadores es irreversible en la práctica y no corresponde a un host prestado.
- **Solo fuera de Development.** En local se trabaja por HTTP; la marca quedaría
  cacheada en el navegador del equipo durante días.

## CA-04, pendiente

`TRUSTED_PROXIES` sigue vacío porque falta medir las IPs de salida del ingress.
Mientras esté vacío, el middleware de forwarded headers no se activa y el límite
por IP agrupa a todos los clientes tras el proxy. El arranque lo advierte.

No bloquea el cierre del Spec: el canal es HTTPS y HSTS está activo. Queda
registrado como deuda en `memoria.md`.

## Cierre

- [x] CA-01, CA-02 y CA-03 satisfechos con evidencia reproducible y pruebas automatizadas.
- [ ] CA-04 — `TRUSTED_PROXIES` sin medir.
- [x] Revisión independiente — verificación en vivo más pruebas de contrato.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

> La cabecera solo se verá en producción cuando se despliegue una imagen que
> incluya este cambio. La 0.3.0 que corre hoy no lo trae.
