# Plan — pm-125-scan-resolve-qr

## Enfoque
- `POST scans/resolve`: cuenta con negocio (403 si no), código o token (formato estricto), límite de 20 intentos por negocio por minuto con 5 minutos de espera (Redis Auth, `scan-business`) y 404 idéntico para desconocido o regenerado. `IPetRepository.FindByQr` es la única lectura sin dueño.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
