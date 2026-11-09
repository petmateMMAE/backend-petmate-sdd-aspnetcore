# Plan — pm-086-user-role-unification

## Enfoque
- `UserRole` queda solo con `User` (valor 1, el de `Owner`, por lo que los datos guardados siguen válidos). Política `RequireUser` (acepta el claim `User` y el anterior `Owner` hasta que expire el token); desaparecen `RequireOwner` y `RequireBusiness`. Ninguna solicitud puede fijar el rol.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
