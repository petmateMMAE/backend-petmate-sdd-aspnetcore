# Plan — pm-087-session-business-link

## Enfoque
- `AuthUserResponse.business` (`{type}` o `null`) calculado por el servidor; siempre `null` mientras no exista el módulo de negocios. Documentado en `docs/api/endpoints.md`.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
