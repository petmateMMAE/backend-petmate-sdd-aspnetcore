# Infraestructura local — Docker (API + 2 Redis) y MongoDB Atlas

> T010/T011 (Specs `pm-012` a `pm-016`; origen: slice histórico `00-foundation`). Fuente normativa: `.specify/memory/constitution.md` (§4 y §5).
> **MongoDB Atlas es el único proveedor de persistencia; Compose no contiene ningún servicio MongoDB.**

## Servicios de Compose (`docker-compose.yml`, proyecto `petmate`)

| Servicio | Imagen | Rol | Persistencia | Puerto publicado |
|---|---|---|---|---|
| `api` | `petmate-api:local` (build del `Dockerfile`, base `dotnet/aspnet:10.0`) | PetMate.Api, usuario no root (UID 1654) | ninguna (stateless) | `127.0.0.1:${API_PORT:-8080}` → 8080 |
| `redis-auth` | `redis:7.4` | Lista negra de JWT (`jwt:blacklist:{jti}`) | AOF `everysec` en volumen `petmate_redis_auth_data` | ninguno |
| `redis-cache` | `redis:7.4` | Caché distribuida | ninguna (tmpfs) | ninguno |

- Red: `petmate_internal` (bridge). Redis no publica puertos; solo es accesible desde la red de Compose.
- `redis-auth`: `maxmemory 64mb`, política `noeviction` (nunca expulsa revocaciones vigentes; si se llena, rechaza escrituras y la API falla cerrado).
- `redis-cache`: `maxmemory 128mb`, política `allkeys-lru`, sin persistencia.
- Ambos: contraseña obligatoria (`requirepass` escrito en `/tmp` tmpfs, no en la línea de comandos), `read_only`, `cap_drop: ALL`, `no-new-privileges`, usuario `redis`.
- Health: Redis con `redis-cli ping`; API con `GET /api/v1/health/live` (liveness). La API espera a `service_healthy`, pero además reconecta sola si Redis cae (`AbortOnConnectFail=false`, conexión perezosa).

## Variables de entorno

| Variable | Quién la usa | Obligatoria | Notas |
|---|---|---|---|
| `MONGODB_URI`, `MONGODB_DATABASE` | API (Atlas) | Sí | Mismo contrato en cualquier entorno. |
| `MONGODB_TEST_URI`, `MONGODB_TEST_DATABASE` | IntegrationTests | Para pruebas Atlas | Base distinta de `MONGODB_DATABASE`. |
| `REDIS_AUTH_CONNECTION`, `REDIS_CACHE_CONNECTION` | API fuera de Compose (ejecución en host) | Sí | Con Compose se derivan de las contraseñas (ver abajo). |
| `REDIS_AUTH_PASSWORD`, `REDIS_CACHE_PASSWORD` | Solo Docker Compose | Sí (Compose) | Una sola vez por servicio; Compose arma las cadenas de la API. |
| `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SIGNING_KEY` | API | Sí | La clave debe ser aleatoria de al menos 256 bits (≥ 32 bytes); fuera de Development se rechazan plantillas (placeholder, ficticio, no_usar…). |
| `JWT_ACCESS_TOKEN_MINUTES` | API | No | Vida del access token; 1–60, por defecto 15. |
| `AUTH_RATE_LIMIT_PER_MINUTE` | API | No | Solicitudes por IP y minuto en register/login/refresh; 1–10000, por defecto 10. |
| `TRUSTED_PROXIES` | API | No | IPs de proxies confiables separadas por coma. Sin ella, tras un proxy la IP observada es la del proxy y el rate limit agrupa a todos los clientes. |
| `ARGON2_MEMORY_KIB`, `ARGON2_ITERATIONS` | API | No | Parámetros de Argon2id; no pueden bajar del mínimo OWASP. |
| `ARGON2_MAX_CONCURRENCY` | API | No | Hashes simultáneos; por defecto max(2, núcleos). Espera máxima de 5 s, después 503. |
| `API_PORT`, `PETMATE_ENVIRONMENT` | Solo Compose | No | Por defecto `8080` y `Development`. |
| `REDIS_AUTH_PORT`, `REDIS_CACHE_PORT` | Solo `docker-compose.debug-ports.yml` | No | Depuración local en loopback. |

El archivo `.env` real está ignorado por Git; `.env.example` solo trae placeholders. La API en Production no lee `.env`.

### TLS en Redis
`InfrastructureSettings` exige `ssl=True` en Redis fuera de Development. En Compose local, las cadenas de la API se generan con `ssl=False` porque el tráfico no sale de la red interna `petmate_internal` y la API corre en Development. **Un Redis en Docker en producción requerirá TLS** (certificados + puerto TLS); eso queda para `pm-070` (Redis Auth) y `pm-071` (Redis Cache) en Azure y no se debilita aquí. **Decisión (2026-11-05, constitución 1.2.0 propuesta):** en Azure cada Redis es una Container App con `--tls-port 6379` y certificado firmado por una CA generada por Terraform; el CD hornea la CA pública en la imagen (`REDIS_CA_CERT_B64`). Ver `azure-deployment.md`.

## Escenarios

- **A — Desarrollo local:** `API + Redis Auth + Redis Cache` en Docker; MongoDB Atlas externo (base de desarrollo). `.env` con `MONGODB_URI` de Atlas.
- **B — Atlas en producción:** misma API y mismo Compose (o equivalente); solo cambia el valor de `MONGODB_URI`/`MONGODB_DATABASE`. Redis permanece en Docker. No hay ramas de código ni perfil para Mongo porque Compose nunca lo incluye.

Con credenciales Atlas en `.env` las pruebas reales pasan y `/ready` reporta `mongodb: Healthy`; sin ellas (o con placeholders) las pruebas Atlas quedan Skipped y `/ready` reporta `mongodb: Unhealthy`.

## Endpoints técnicos

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/v1/health/live` | Liveness; 200 sin consultar dependencias. |
| GET | `/api/v1/health/ready` | Readiness; 200 si MongoDB, Redis Auth y Redis Cache responden; 503 con la lista `data.dependencies[{name,status}]` si alguno falla. Sin hosts, URIs ni mensajes de excepción. |

## Comandos (PowerShell o Git Bash, desde la raíz del repositorio)

```bash
docker compose up -d --build
```

```bash
docker compose ps
```

```bash
docker compose logs -f api
```

```bash
curl http://127.0.0.1:8080/api/v1/health/ready
```

```bash
docker compose stop
```

`stop`/`start` conservan los volúmenes (AOF de Redis Auth). `docker compose down -v` **elimina** el volumen: no ejecutarlo sin autorización. Con Compose se usa la variable `COMPOSE_PROJECT_NAME`/`name: petmate`, por lo que no interfiere con otros proyectos.

## Pruebas

Las suites que dependen de Redis se omiten si no existen `PETMATE_IT_REDIS_AUTH` y `PETMATE_IT_REDIS_CACHE` (cadenas con contraseña, p. ej. `redis-auth:6379,password=...`). Para ejecutarlas dentro de la red de Compose:

```bash
docker run --rm --network petmate_internal -e PETMATE_IT_REDIS_AUTH -e PETMATE_IT_REDIS_CACHE -v "$PWD:/src:ro" mcr.microsoft.com/dotnet/sdk:10.0 bash -c "cp -r /src /work && cd /work && dotnet test tests/PetMate.IntegrationTests -c Release"
```

En este equipo, Smart App Control bloquea los binarios nuevos en el host (ver `Memoria.md`); por eso la evidencia se obtuvo en contenedores.
