# Plan — pm-119-business-public-profile

## Enfoque
- `GET businesses/get-by-id/{id}` anónimo: perfil público (descripción, foto, horario, ubicación, servicios hasta 100 y contacto; un contacto ausente es `null`). La proyección pública no contiene la cédula profesional. Inexistente o dado de baja → 404.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites y guardas de este hito a confirmar: radio máximo 50 km, búsqueda de 2 a 50 caracteres, 20 escaneos por minuto por negocio, 120 lecturas públicas por minuto por IP.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas; la caché usa Redis Cache ya existente.
