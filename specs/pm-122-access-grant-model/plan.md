# Plan — pm-122-access-grant-model

## Enfoque
- `AccessGrant` (mascota, dueño, negocio opcional = cualquier negocio, grupos `Vaccines`/`Dewormings`/`Weights`/`Consultations`, vencimiento; las alergias siempre van) en `access_grants` con índice por mascota y vencimiento y TTL de un día tras vencer. Sin endpoint de listado ni revocación.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
