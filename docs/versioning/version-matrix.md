# Matriz de versionamiento — PetMate Backend

> Requisito 11 de los lineamientos del proyecto móvil: «Previo al desarrollo se deberá tener una matriz de numeración de versionamiento y aplicar en cada despliegue». Spec: `pm-059`. Constitución §8.12 y §8.13.

## Esquema

- **SemVer `MAJOR.MINOR.PATCH`**.
  - `MAJOR`: cambio incompatible del contrato público (rutas `/api/v1` → `/api/v2`, wrapper, autenticación).
  - `MINOR`: endpoints o capacidades nuevas compatibles.
  - `PATCH`: correcciones sin cambio de contrato.
- Mientras el producto no llegue a la entrega final se usa `0.y.z`; `1.0.0` es la versión de la evaluación del 2 de diciembre de 2026.
- La versión del contrato HTTP va en la ruta (`/api/v1/...`) y solo cambia con un `MAJOR`.

## Trazabilidad versión → etiqueta → imagen → despliegue

| Elemento | Formato | Ejemplo |
|---|---|---|
| Etiqueta Git (dispara el CD) | `api-vX.Y.Z` | `api-v0.1.0` |
| Imagen en ACR (inmutable) | `petmate-api:X.Y.Z-<sha7>` | `petmate-api:0.1.0-4f9bb02` |
| Revisión de Container Apps | `petmate-api--vX-Y-Z-<run>` | `petmate-api--v0-1-0-12` |
| Variable en el contenedor | `PETMATE_VERSION=X.Y.Z` | `0.1.0` |

- La etiqueta solo se crea sobre un commit de `main` que pasó el pipeline (`.github/workflows/pipeline.yml`: puerta de calidad, imagen y despliegue).
- La imagen se bloquea contra escritura y borrado al publicarse (`az acr repository update --write-enabled false`).
- El CD (job `deploy` de `.github/workflows/pipeline.yml`) escribe en el resumen del job la URL, la imagen y las réplicas.

## Matriz

| Versión | Contenido | Specs | Estado |
|---|---|---|---|
| `0.1.0` | Primer despliegue en Azure: 7 endpoints actuales (`auth/register`, `auth/login`, `auth/refresh`, `auth/logout`, `health/live`, `health/ready`, `technical/mediator-probe`), 2 instancias, balanceo, HTTPS, Atlas, Redis Auth y Redis Cache, logs | pm-001…pm-032, pm-055…pm-078, pm-083, pm-084 | **Desplegada** 2026-11-06 (Chile Central) |
| `0.1.x` | Correcciones del primer despliegue (configuración, TLS de Redis, proxies) sin cambiar contrato | — | Reservada |
| `0.2.0` | Autorización por rol (denegación por defecto) y perfil propio: `GET users/me/get`, `PUT users/me/update` (nombre visible). 9 endpoints | pm-033, pm-037, pm-038 | **Desplegada** 2026-11-06 (Chile Central) |
| `0.3.0` | Mascotas: modelo, alta, lista, detalle, edición y borrado físico (cierre del primer hito, 14 endpoints) | pm-039…pm-047 | Planeada |
| `0.4.0` | Rendimiento de API: caché en backend, paginación, compresión, serialización | pm-047…pm-053 | Planeada |
| `0.5.0` | Imágenes en Blob Storage + CDN, moderación de contenido | pm-079…pm-082 | Planeada |
| `0.6.0` | Funciones restantes del producto (negocios, consultas, seguimiento, notificaciones, eliminar cuenta) | specs por crear | Planeada |
| `1.0.0` | Versión de la evaluación final (2 de diciembre de 2026) | pm-084 | Planeada |

## Reglas

1. Cada despliegue a producción tiene una versión de esta matriz; no se despliega `latest`.
2. Cambiar la matriz (agregar o mover contenido entre versiones) se registra en `Memoria.md`.
3. La reversa vuelve a una imagen ya publicada (misma versión anterior); no se reetiqueta una imagen existente.
4. La app móvil lleva su propia matriz en su repositorio y declara con qué versión mínima de la API funciona.
