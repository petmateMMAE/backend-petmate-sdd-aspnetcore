# Gates — pm-084-cloud-deployment-acceptance (T096)

> Estado (2026-11-19): **cerrado**. Aceptación del sistema desplegado,
> verificada contra producción en vivo.

Esta Spec cierra el bloque de despliegue: comprueba que lo que las demás
describen por separado funciona junto.

## CA-01 — Dos instancias, balanceador y HTTPS

```
$ az containerapp show --name petmate-api --resource-group rg-petmate-prod
minReplicas:    2
external:       True
allowInsecure:  False
image:          acrpetmateprodd8u3u.azurecr.io/petmate-api:0.5.0-77760e1

$ az containerapp replica list --name petmate-api --resource-group rg-petmate-prod --query "length(@)"
2
```

**Dos réplicas vivas**, no solo configuradas. El reparto entre ellas se midió en
`pm-076`: 40 peticiones repartidas 20/20.

HTTPS: `allowInsecure: False`, HTTP responde 301 hacia HTTPS y desde el
2026-11-19 la API declara HSTS con `max-age` de un año (`pm-073`).

| Prueba | Resultado |
|---|---|
| `GET /api/v1/health/live` | 200 |
| `GET /api/v1/health/ready` (verifica Mongo y Redis) | 200 |
| `POST /api/v1/pets/upload-photo/{id}` | 401 (pide autenticación, ya no 503) |
| `http://` → `https://` | 301 |

## CA-02 — CI, CD y observabilidad operativos

**CI:** el pipeline corre 86 jobs por PR —secretos, SAST, integridad SDD,
unitarias, seguridad, contrato por endpoint, integración, puerta de calidad,
imagen con Trivy y SBOM—. El PR #31 cerró con 86 en verde.

**CD:** el despliegue a `main` del 2026-11-20T01:16 terminó en `success` con el
commit `77760e1`, que es exactamente la imagen que corre en producción
(`0.5.0-77760e1`). La prueba de humo posterior pasó.

**Observabilidad:** Log Analytics `log-petmate-prod` recibe los registros, y hay
dos alertas activas (`pm-078`): errores 5xx (severidad 2) e indisponibilidad por
réplicas < 2 (severidad 1).

## CA-03 — Costo

```
$ az consumption usage list --start-date 2026-11-01 --end-date 2026-11-20
gasto del mes: 0.00 USD

$ az consumption budget list
budget-petmate-prod   40.0 USD   Monthly
```

**Desembolso directo: $0.00 MXN.** Todo el consumo se cubre con el crédito de
Azure for Students; no hay tarjeta asociada a la suscripción, así que un
sobregiro deshabilita la suscripción en vez de generar cargo.

El presupuesto está en USD y el criterio pide MXN. Con desembolso de cero, la
conversión es cero en cualquier moneda, así que el criterio se satisface sin
ambigüedad.

### Conflictos de costo documentados, sin reducir requisitos

Tres Specs se cerraron con el conflicto de costo que ellas mismas autorizan:

| Spec | Qué se difirió | Qué se conserva |
|---|---|---|
| `pm-070`, `pm-071` | Azure Cache for Redis (~32 USD/mes) | TLS con el puerto en claro apagado, autenticación, red privada, secretos rotables |
| `pm-082` | Azure CDN / Front Door (~35 USD/mes) | Fotos públicas servidas directo del Blob; el contenedor privado sigue privado |

Ninguna reduce un requisito de seguridad: se difiere el servicio administrado,
no la propiedad que la Spec evalúa.

## CA-04 — Aprobación humana

- [x] **Adrián Adonai García Jiménez, 2026-11-19.**

El sistema desplegado se revisó contra producción en vivo, no contra el
repositorio: réplicas, reparto, HTTPS, salud con dependencias, fotos, alertas y
costo.

## Cierre

- [x] CA-01 — dos instancias vivas, balanceador y HTTPS verificados.
- [x] CA-02 — CI con 86 jobs, CD exitoso y observabilidad con dos alertas.
- [x] CA-03 — $0.00 de desembolso directo; conflictos documentados.
- [x] CA-04 — aprobación humana registrada.
- [x] Revisión independiente — medición en vivo contra el entorno desplegado.

## Lo que queda abierto, y no bloquea

`pm-077#CA-03` (ensayar la reversa) necesita dos revisiones activas y una
ventana fuera del horario de los testers de Google Play. El mecanismo está
declarado en el workflow; falta el ensayo.

`pm-076#CA-03` (retirar una réplica) es la misma situación: prueba de
resiliencia pendiente de ventana acordada.

Ninguna impide aceptar el despliegue: lo que se exige aquí es que el sistema
funcione y esté observado, y ambas cosas están demostradas.
