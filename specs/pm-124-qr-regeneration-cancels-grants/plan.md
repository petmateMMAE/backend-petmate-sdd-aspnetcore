# Plan — pm-124-qr-regeneration-cancels-grants

## Enfoque
- `RegeneratePetQrHandler` cancela todas las autorizaciones vigentes de la mascota antes de cambiar el QR, de modo que un fallo intermedio nunca deja una autorización viva con un QR nuevo. El QR anterior ya no resuelve (404).
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
