# Plan — pm-090-password-reset-confirm

## Enfoque
- `POST auth/reset-password`: política de contraseña 12 a 128; el hash Argon2id se calcula antes de consumir el token; consumo atómico (`FindOneAndUpdate`); error único para token usado, vencido o inexistente (400); al cambiar la contraseña se revocan todas las sesiones de la cuenta (`RevokeAllForUser`) y sus access tokens (lista negra por sesión).
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
