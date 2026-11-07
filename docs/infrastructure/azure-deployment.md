# Despliegue en Azure — primer despliegue del backend actual

> Alcance aprobado por el equipo (2026-11-05): publicar **exactamente el backend actual** (7 endpoints) con dos instancias, balanceo, HTTPS, Atlas, Redis Auth y Redis Cache, secretos, logs, CI/CD y Terraform. Sin endpoints nuevos, sin imágenes ni CDN.
> Decisiones: `docs/decisions/ADR-003-azure-container-apps.md` y `ADR-004-terraform.md`. Versiones: `docs/versioning/version-matrix.md`. Constitución §8 (v1.2.0 propuesta).

## 1. Arquitectura

```mermaid
flowchart LR
  C[App móvil / navegador] -->|HTTPS 443| IN[Ingress de Container Apps<br/>balanceador + TLS]
  IN --> A1[petmate-api réplica 1]
  IN --> A2[petmate-api réplica 2]
  A1 & A2 -->|TLS 6379, VNet| RA[(redis-auth<br/>noeviction + AOF)]
  A1 & A2 -->|TLS 6379, VNet| RC[(redis-cache<br/>allkeys-lru)]
  RA --- F[(Azure Files)]
  A1 & A2 -->|TLS, lista de IPs| AT[(MongoDB Atlas)]
  A1 & A2 --> LA[Log Analytics + alertas]
  GH[GitHub Actions] -->|OIDC| ACR[ACR Basic] --> A1 & A2
```

Todo lo de la app vive en el grupo de recursos **`rg-petmate-prod`**. El estado de Terraform vive aparte en `rg-petmate-tfstate` para que la destrucción nunca lo borre.

## 2. Endpoints publicados (v0.1.0)

| Método | Ruta | Dependencias |
|---|---|---|
| POST | `/api/v1/auth/register` | Atlas, Argon2id, JWT |
| POST | `/api/v1/auth/login` | Atlas, Argon2id, JWT |
| POST | `/api/v1/auth/refresh` | Atlas, JWT |
| POST | `/api/v1/auth/logout` | Atlas, Redis Auth (lista negra), JWT |
| GET | `/api/v1/health/live` | ninguna |
| GET | `/api/v1/health/ready` | Atlas, Redis Auth, Redis Cache |
| GET | `/api/v1/technical/mediator-probe` | Mediator |

La URL pública queda como `https://petmate-api.<dominio-del-ambiente>.<region>.azurecontainerapps.io` (salida `api_url` de Terraform).

**Estado (2026-11-06 10:40):** v0.1.0 desplegada en **Chile Central** — `https://petmate-api.purplefield-b84659a6.chilecentral.azurecontainerapps.io`. Dos réplicas `Running`, `health/ready` con `mongodb`, `redis-auth` y `redis-cache` en `Healthy`, prueba de humo de los 7 endpoints aprobada (run 42253209567) y etiqueta `api-v0.1.0`.

### 2.1 Por qué Chile Central y no México Central

La región planeada era `mexicocentral` (la más cercana a los usuarios). No se pudo usar:

| Fecha y hora (México) | Intento | Resultado |
|---|---|---|
| 05/11 22:21 | Apply 1 | El ambiente de Container Apps quedó en `Waiting`; Terraform se cortó a los 30 min (`context deadline exceeded`). Azure lo siguió en `Waiting` más de 1 h sin errores; se borró a mano a las 23:21. |
| 05/11 23:43 → 06/11 05:06 | Intentos 2 a 9 (8 intentos automáticos con `scripts/watch-deploy.ps1` y el orquestador) | En **todos** el ambiente se quedó en `Waiting` sin ningún evento nuevo en Azure. Cada intento se canceló tras 20 min sin avance, se borró el ambiente (≈15–20 min cada borrado) y se reintentó. |
| 06/11 05:22 | Cambio a `chilecentral` (regla acordada: si a las 5:00 no levanta México) | Primer apply falló por nombres repetidos (ACR y Log Analytics de México aún en borrado: `InvalidResourceLocation`). |
| 06/11 06:20 | Apply en Chile | El ambiente se creó en **~5 min**; Redis y API en ~3 min más. **Éxito.** |

**Total: 9 intentos de crear el ambiente en México Central en ~7 horas, ninguno terminó.** No hubo errores de configuración: la subred estaba delegada a `Microsoft.App/environments`, los proveedores registrados y la misma configuración funcionó a la primera en Chile. El patrón coincide con problemas de capacidad regional reportados por Microsoft en Container Apps (p. ej. `ManagedEnvironmentCapacityHeavyUsageError` en Central US desde el 30/09/2026) y con la recomendación de su soporte: un ambiente en `Waiting` rara vez se recupera; hay que borrarlo y reintentar o usar otra región.

Regiones que permite la política de Azure for Students en esta suscripción: `belgiumcentral`, `chilecentral`, `francecentral`, `mexicocentral`, `norwayeast`. Chile es la más cercana después de México.

**Impacto de Chile:** latencia estimada ~0.2 s por petición sencilla y ~0.7 s por login (contra ~0.05 s y ~0.3 s en México), por la distancia a los usuarios y a Atlas (Azure `eastus2`). Aceptable para pruebas y evaluación. Se puede volver a México cuando la región tenga capacidad: variable `AZURE_LOCATION=mexicocentral`, `destroy` y `apply` (Atlas conserva los datos).

### 2.2 Despliegue automático y modo ahorro

- **Cada merge a `main` que toque el código** (`src/`, `Dockerfile`, `Directory.*.props` o `VERSION`) dispara el pipeline *CI/CD DevSecOps* (ver [docs/ci-cd/pipeline.md](../ci-cd/pipeline.md)); tras la puerta de calidad el job de despliegue construye, escanea con Trivy, publica la imagen `X.Y.Z-<sha7>`, crea una revisión nueva, corre la prueba de humo de los 7 endpoints y, si falla, regresa sola a la imagen anterior. Tarda **~9 min** (construir ~3, Trivy ~1, publicar ~1, revisión ~2, prueba ~1). La versión se toma del archivo `VERSION`; al cambiarla se crea la etiqueta `api-vX.Y.Z`. Cambios solo de documentación no despliegan.
- **Modo ahorro** (`MODO_AHORRO=true`, variable de GitHub + *Infraestructura → apply*): una vez aplicado es automático — la API se apaga tras 15 min sin peticiones y se prende sola con la siguiente (~10–20 s la primera), y Redis se apaga de 23:00 a 7:00. Un merge a `main` en ese horario fallará la prueba de humo y hará la reversa: para desplegar de noche, aplicar antes `MODO_AHORRO=false`.

## 3. Configuración de producción (sin cambios de código)

| Variable | Origen | Valor |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Terraform | `Production` (no lee `.env`; OpenAPI/Scalar apagados) |
| `MONGODB_URI` | secreto GitHub `TF_MONGODB_URI` → secreto Container Apps | usuario de Atlas con `readWrite` solo en `petmate_prod` |
| `MONGODB_DATABASE` | Terraform | `petmate_prod` |
| `REDIS_AUTH_CONNECTION` / `REDIS_CACHE_CONNECTION` | generadas por Terraform | `redis-auth:6379,password=…,ssl=True,sslHost=redis-auth,abortConnect=False` |
| `JWT_ISSUER` | Terraform | `https://<fqdn de la API>` |
| `JWT_AUDIENCE` | Terraform | `petmate-api` |
| `JWT_SIGNING_KEY` | secreto GitHub `TF_JWT_SIGNING_KEY` | `openssl rand -base64 48` (nunca la del `.env`) |
| `TRUSTED_PROXIES` | variable GitHub `TRUSTED_PROXIES` | vacía hasta medir las IPs del ingress |
| `PETMATE_VERSION` | CD | versión SemVer desplegada |

Confianza en el TLS de Redis: el CD hornea el certificado público de la CA en la imagen (`--build-arg REDIS_CA_CERT_B64`, ver `Dockerfile`) y .NET lo usa desde el almacén del sistema. Compose y el desarrollo local no cambian.

## 4. Estudio de costos (pm-062)

Consulta: 2026-11-05. Precios de lista en USD; **confirmar en la calculadora de Azure** antes del primer `apply` y registrar el resultado aquí. Moneda de referencia de la constitución: MXN.

| Componente | Servicio | Clasificación (§8.2) | Estimado USD/mes | Fuente |
|---|---|---|---|---|
| 2 réplicas de la API (0.25 vCPU / 0.5 GiB) | Container Apps Consumption | crédito (tras la concesión gratuita de 180 000 vCPU-s y 360 000 GiB-s al mes) | ~9 | [Billing](https://learn.microsoft.com/azure/container-apps/billing), [Precios](https://azure.microsoft.com/pricing/details/container-apps/) |
| redis-auth y redis-cache (0.25 vCPU / 0.5 GiB c/u) | Container Apps Consumption | crédito | ~10 | idem |
| Balanceador, TLS y certificado | Ingress del ambiente | incluido | 0 | [Ingress](https://learn.microsoft.com/azure/container-apps/ingress-overview) |
| VNet y subred | Virtual Network | incluido | 0 | — |
| Registro de imágenes | ACR Basic (10 GiB incluidos) | crédito | ~5 | [ACR](https://azure.microsoft.com/pricing/details/container-registry/) |
| AOF de Redis Auth + estado de Terraform | Storage (Files 1 GiB, Blob) | crédito | < 1 | — |
| Logs y alertas por correo | Log Analytics (tope 0.15 GB/día), Azure Monitor | crédito / incluido | ~1 | — |
| Presupuesto | Cost Management | incluido | 0 | — |
| Base de datos | MongoDB Atlas M0 (externo) | incluido | 0 | — |
| CI/CD | GitHub Actions (repositorio privado, 2 000 min/mes gratis) | incluido | 0 | — |
| **Total** | | | **~26–28 USD/mes** (~1 USD/día) | |

- **Modo ahorro (por defecto en pruebas):** API ~0–1, Redis ~6–7 (16 h/día), ACR ~5, logs ~1–2 → **~13–15 USD/mes**. La API se apaga tras 15 min sin peticiones y Redis de 23:00 a 7:00. Para una revisión: variable de GitHub `MODO_AHORRO=false` → *Infraestructura → apply*; al terminar, quitarla y volver a aplicar.
- Crédito disponible: Azure for Students (USD 100 por 12 meses; sin tarjeta; al agotarse la suscripción se deshabilita, no genera cargos) — [FAQ](https://learn.microsoft.com/azure/education-hub/faq).
- El presupuesto de Terraform (USD 40/mes) avisa al 50, 80 y 100 %.
- **Sin conflicto académico:** la arquitectura completa (2 instancias + balanceador) cabe en el crédito; no se reduce ningún requisito (pm-062#CA-04).
- Descartados por costo: NAT Gateway (~33 USD/mes), Front Door (~35 USD/mes de base), Redis administrado ×2 con private endpoints.

## 5. Pasos hasta tener los 7 endpoints por HTTPS

1. **Herramientas** en la máquina de quien despliega: Azure CLI, Terraform ≥ 1.9, Git (`winget install Microsoft.AzureCLI Hashicorp.Terraform`).
2. **Verificar la suscripción** sin crear nada:
   `./infra/bootstrap/bootstrap.ps1 -SubscriptionId <id> -Location chilecentral -CheckOnly`
   (regiones permitidas por la política de Azure for Students y disponibilidad de Container Apps).
3. **Bootstrap** (con autorización): mismo comando sin `-CheckOnly`. Crea `rg-petmate-prod`, `rg-petmate-tfstate`, la cuenta del estado y la identidad OIDC; imprime las variables de GitHub.
4. **Atlas**: crear la base `petmate_prod`, un usuario con `readWrite` solo en ella y una API key del proyecto (Project IP Access List Admin). Opcional: correr `scripts/mongo/petmate-db.mongodb.js`.
5. **GitHub** (Settings → Secrets and variables → Actions): variables que imprime el bootstrap y secretos `TF_MONGODB_URI`, `TF_JWT_SIGNING_KEY`, `ALERT_EMAIL`, `ATLAS_PROJECT_ID`, `MONGODB_ATLAS_PUBLIC_KEY`, `MONGODB_ATLAS_PRIVATE_KEY`, `MONGODB_TEST_URI`, `MONGODB_TEST_DATABASE`. Sin environments (plan gratuito): los workflows solo corren desde `main`.
6. **Plan**: Actions → *Infraestructura* → `plan` (rama `main`). Revisar con el equipo.
7. **Apply** (con autorización): Actions → *Infraestructura* → `apply`. Crea el ambiente, ACR, los dos Redis, la API (con imagen de arranque) y la lista de acceso de Atlas.
8. **Primera versión**: Actions → *CI/CD DevSecOps* → Run workflow → versión `0.1.0` (rama `main`): construye, escanea, publica, despliega, ejecuta `scripts/smoke-test.sh` y crea la etiqueta `api-v0.1.0`. En modo ahorro hacerlo entre 7:00 y 23:00.
9. **Verificar** (pm-084): las 2 réplicas en el resumen del job; `ready` con `mongodb`, `redis-auth` y `redis-cache` en `Healthy`; logout revoca el token; HTTP redirige a HTTPS; reinicio de `redis-auth` sin perder revocaciones vigentes.
10. **Proxies**: en Log Analytics revisar la IP de origen que ve la API; si es estable, registrar `TRUSTED_PROXIES` y volver a aplicar.
11. Registrar la evidencia en `checklists/gates.md` de los specs y en `Memoria.md`.

## 6. Operación

- **Logs**: Log Analytics → tablas `ContainerAppConsoleLogs_CL` y `ContainerAppSystemLogs_CL` (filtrar por `ContainerAppName_s == "petmate-api"`).
- **Reversa**: el CD vuelve solo a la imagen anterior si falla la prueba de humo. A mano: `az containerapp update -n petmate-api -g rg-petmate-prod --image <imagen-anterior>`.
- **Destruir y recrear**: Actions → *Infraestructura* → `destroy` (escribir `destruir-petmate`). Atlas conserva sus datos; `apply` vuelve a crear todo.
- **IPs de Atlas**: *Sincronizar IPs de Atlas* corre cada 6 horas y a mano.

## 7. Riesgos conocidos

| Riesgo | Mitigación |
|---|---|
| La política de Azure for Students no permite la región o Container Apps | `bootstrap.ps1 -CheckOnly` antes de crear nada; cambiar `-Location` |
| El tenant escolar no deja registrar la aplicación OIDC | Pedir permiso al administrador o usar un service principal creado por quien tenga el rol; documentarlo |
| `TRUSTED_PROXIES` sin rango CIDR: el rate limit se agrupa por proxy | Medir IPs en el paso 10; si no son estables, spec de cambio para soportar CIDR (requiere aprobación) |
| Montaje de Azure Files restringido a la subred | Si el montaje falla, permitir el acceso del ambiente y documentarlo como excepción |
| Cambio de IPs de salida antes de la sincronización | Sincronización cada 6 h y manual; readiness lo refleja como `mongodb: Unhealthy` |
| HSTS no implementado (constitución §8.7) | Brecha registrada; requiere spec propio |
