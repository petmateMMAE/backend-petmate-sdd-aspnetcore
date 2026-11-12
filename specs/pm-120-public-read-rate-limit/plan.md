# Plan — pm-120-public-read-rate-limit

## Enfoque
- Filtro `PublicBusinessIpThrottleFilter` con el contador de Redis Auth (`public-business-ip`) más el limitador en proceso (`public-business`); 120 solicitudes por minuto por IP (`PUBLIC_BUSINESS_RATE_LIMIT_PER_MINUTE`, 1–10000). Falla cerrado igual que `pm-036#CA-05`. Matriz de límites en `docs/api/endpoints.md`.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
