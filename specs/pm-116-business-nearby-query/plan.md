# Plan — pm-116-business-nearby-query

## Enfoque
- `GET businesses/get-nearby` anónimo con `$geoNear` sobre el índice 2dsphere (distancia en metros, radio de 5 km por defecto y máximo de 50), orden por distancia e identificador y paginación de `pm-047`. Cada elemento lleva foto (nula hasta `pm-111`), nombre, tipo, «abierto ahora», horario de hoy y distancia. El tiempo de respuesta frente al objetivo de 2 segundos (RNF-10) se mide en la línea base de rendimiento con Atlas; hoy no se pudo medir (Atlas sin conexión).
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
