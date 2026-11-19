# Plan — pm-165-account-deletion-web-confirm

## Diseño (pm-165#CA-01 a CA-03)
- `POST /api/v1/account-deletion/confirm` con `{ "token": "..." }` (público, con el límite por IP de las rutas de autenticación). Un token vigente se consume de forma atómica y ejecuta el mismo `AccountEraser` que `users/me/delete` (pm-161 a pm-163), sin pedir contraseña (CA-01).
- **CA-02:** token usado, vencido, inexistente, de otro flujo (un token de restablecimiento no sirve) o de una cuenta ya borrada devuelven el mismo 400 «El enlace no es válido o ya venció. Solicita uno nuevo.».
- **CA-03:** el borrado revoca todas las sesiones y sus access tokens antes de borrar los datos.
- Sin paquetes nuevos.

## Verificación
- Contrato: `Endpoint=account-deletion-confirm` (borra todo una sola vez, el replay da 400, refresh y login fallan después, token ajeno al flujo rechazado, 400 por cuerpo inválido).
