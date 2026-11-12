# Plan — pm-117-business-type-filter

## Enfoque
- Parámetro `types` con valores `Veterinary`, `Store`, `Grooming` separados por comas; un valor inválido devuelve 400 y sin parámetro se devuelven todos. Filtro `$in` dentro del `$geoNear`.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
