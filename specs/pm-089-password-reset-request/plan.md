# Plan — pm-089-password-reset-request

## Enfoque
- `POST auth/forgot-password`: respuesta 200 idéntica exista o no el correo; cuota por correo de 3 solicitudes en 15 minutos (cuentan existan o no) más el límite por IP de las rutas de autenticación; token de 256 bits (misma fábrica que el refresh), solo se guarda el hash (`password_resets`, índice único y TTL), vigencia de 1 hora. Limitación documentada: la llamada al proveedor hace algo más lenta la solicitud de una cuenta existente.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
