# Plan — pm-168-business-get-own

## Enfoque
- `GET /api/v1/businesses/get` en `BusinessesController` (política `business`, 60 solicitudes/min por usuario, `BUSINESS_RATE_LIMIT_PER_MINUTE`, como el resto del panel). Sin parámetros: la cuenta sale del claim `sub`.
- Caso de uso `GetOwnBusinessQuery`/`GetOwnBusinessHandler`: reutiliza `IBusinessRepository.FindByOwner` y `BusinessResult.From`; devuelve `NotFound` si la cuenta no tiene negocio. El controlador lo traduce a 404 (CA-03), distinto del 403 que usan las escrituras del panel.
- Respuesta: el mismo `BusinessResponse` de `create`/`update` (incluye `id` y `licenseNumber`, privada del dueño).
- Pruebas: contrato (`businesses-get`: 200 con cédula e id, 404 de otra cuenta sin negocio, 401); la identidad siempre del token se ejerce con dos cuentas. Sin integración nueva: no añade consultas de persistencia.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md`.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Solo lectura sobre MongoDB Atlas; sin escritura ni invalidación de caché.
