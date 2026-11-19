# Plan — pm-128-scan-ficha-basic

## Enfoque
- La ficha básica (foto, nombre, especie, raza, edad y alergias activas) se entrega a cualquier negocio autorizado y no contiene ningún otro campo (prueba de contrato). `petId` se añade solo a una veterinaria. La ficha completa de la veterinaria (`pm-127`, T145) queda pendiente de las consultas veterinarias.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
