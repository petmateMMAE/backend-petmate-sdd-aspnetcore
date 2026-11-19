# Plan — pm-118-business-name-search

## Enfoque
- Parámetro `search` (2 a 50 caracteres) contra el campo `nameSearch` (minúsculas y sin acentos, calculado al guardar). El texto se escapa con `Regex.Escape` y se usa como dato, nunca como expresión. Se combina con tipo y cercanía.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
