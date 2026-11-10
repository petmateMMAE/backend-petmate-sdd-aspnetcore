# Plan — pm-085-account-registration-name

## Enfoque
- El registro exige `displayName` (1 a 80 caracteres, mismo validador que el perfil); `User.Register` lo normaliza y lo guarda; la política de contraseña no cambia (12 a 128). La sesión devuelve el nombre en `user.displayName`.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
