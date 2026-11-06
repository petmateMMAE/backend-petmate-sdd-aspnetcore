# ADR-003 — Servicios de Azure para el primer despliegue

- **Estado:** Propuesto (pendiente de aprobación humana en el PR)
- **Fecha:** 2026-11-05
- **Specs:** pm-063 (principal), pm-062, pm-068, pm-070, pm-071, pm-072, pm-073, pm-074, pm-075, pm-076, pm-078
- **Constitución:** §8 (v1.2.0 propuesta)

## Contexto

El primer despliegue publica el backend actual (7 endpoints) en Azure for Students con: dos instancias, balanceo, HTTPS con certificado válido, MongoDB Atlas, Redis Auth y Redis Cache independientes, secretos de producción, observabilidad, CI/CD y Terraform. El objetivo económico es $0 MXN de desembolso directo usando el crédito de Azure for Students.

## Decisión

| Componente | Servicio | Motivo |
|---|---|---|
| Cómputo de la API (2 instancias) | Azure Container Apps, ambiente *workload profiles* con perfil **Consumption**, `min = max = 2` réplicas | Contenedores sin administrar servidores; concesión gratuita mensual; las réplicas en reposo se cobran a tarifa reducida |
| Balanceador y HTTPS | Ingress administrado del ambiente (Envoy) | Reparte entre las réplicas, termina TLS 1.2/1.3 con certificado válido del dominio `*.azurecontainerapps.io` y redirige el puerto 80 a 443; sin costo adicional |
| Redis Auth y Redis Cache | Dos Container Apps con `redis:7.4`, ingress **TCP interno** (sin entrada desde Internet), TLS con CA propia y contraseña distinta | Azure Cache for Redis (Basic/Standard/Premium) se retira el 30/09/2028 y su reemplazo no tiene plan gratuito; dos instancias administradas con red privada exceden el presupuesto. **Requiere la enmienda §8.9 de la constitución v1.2.0** |
| Persistencia de Redis Auth | AOF en un share de Azure Files (1 GiB) | `noeviction` + persistencia exigidos por §5.1 y pm-070 |
| Red privada | VNet propia con subred delegada `/24`; storage restringido a la subred | §8.8: los datos no son accesibles desde Internet |
| Registro de imágenes | Azure Container Registry **Basic**, sin usuario admin; pull por identidad administrada (AcrPull) | §8.12; etiquetas inmutables |
| Secretos | Secretos de Container Apps, cargados por Terraform desde secretos de GitHub | §8.11 sin Key Vault (evita costo y complejidad) |
| Observabilidad | Log Analytics (logs de consola y del sistema) + alerta de 5xx + presupuesto con alertas | §8.14; tope diario de ingesta 0.15 GB |
| Base de datos | MongoDB Atlas (servicio externo permitido por los lineamientos) con lista de acceso = IPs de salida de la API, sincronizada por Terraform | Sin `0.0.0.0/0`; NAT Gateway (IP fija) se descarta por costo |
| Fuera de este alcance | Blob Storage, CDN/Front Door, Key Vault | No hay endpoints de imágenes todavía (pm-079…pm-082) |

## Alternativas descartadas

- **App Service** con 2 instancias: requiere plan de pago (Basic o superior) todo el mes.
- **AKS**: costo y operación mayores que el alcance.
- **Azure Managed Redis / Azure Cache for Redis**: costo de dos instancias + private endpoints.
- **NAT Gateway** para IP fija hacia Atlas: unos USD 33 al mes adicionales.
- **Azure Front Door** como balanceador: no se necesita para balancear ni para HTTPS (lo hace el ingress); base de USD 35 al mes.

## Consecuencias

- La API debe confiar en la CA propia de Redis: el CD la hornea en la imagen (`REDIS_CA_CERT_B64` en el `Dockerfile`), sin cambiar código.
- `TRUSTED_PROXIES` solo acepta IPs; las del ingress se miden en el primer despliegue. Hasta entonces el rate limit de autenticación se agrupa por IP del proxy (riesgo documentado).
- El contenedor de Redis corre como root dentro de su contenedor (Container Apps no permite fijar el usuario sin una imagen propia); está aislado en la red interna.
- Las IPs de salida pueden cambiar; `.github/workflows/atlas-ip-sync.yml` las vuelve a sincronizar cada 6 horas.
- Costo estimado en `docs/infrastructure/azure-deployment.md` (pm-062).
- **Modo ahorro** (`modo_ahorro`, aprobado por el equipo para la fase de pruebas): API de 0 a 2 réplicas por peticiones con apagado tras 15 min sin uso, y Redis con horario 7:00–23:00 (regla cron). Baja el costo de ~27 a ~14 USD/mes; primera petición ~10–20 s; sin servicio de noche. Para revisiones se aplica `modo_ahorro = false`.
- **GitHub plan gratuito:** la organización no tiene environments en repositorios privados, así que los workflows de infraestructura y despliegue se disparan a mano solo desde `main`, y la identidad OIDC de Azure solo confía en `ref:refs/heads/main`.
