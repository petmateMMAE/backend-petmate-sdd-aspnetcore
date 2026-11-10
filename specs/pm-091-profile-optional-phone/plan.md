# Plan — pm-091-profile-optional-phone

## Enfoque
- `User.Phone` opcional de 10 dígitos (se admiten espacios, guiones, paréntesis y +52; se guarda normalizado); `PUT users/me/update` reemplaza el perfil completo, por lo que un `phone` ausente, nulo o vacío lo quita; no se pide en el registro ni se verifica por SMS; solo lo devuelve `me/get` a su propietario.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
