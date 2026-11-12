# Plan — pm-121-business-public-cache

## Enfoque
- Caché Redis Cache de las lecturas públicas con TTL de 5 minutos: el mapa cambia poco y cada cambio invalida la caché (versión de clave), por lo que el TTL solo acota un dato viejo si la invalidación falla. «Abierto ahora» no se guarda; las coordenadas se redondean a 3 decimales. Un fallo de la caché no afecta la respuesta. `docs/security/cache-policy.md` y `CachePolicyTests` ampliados (pm-051).
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
