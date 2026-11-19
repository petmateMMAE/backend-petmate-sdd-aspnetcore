# Plan — pm-161-account-delete-request

## Diseño (pm-161#CA-01 a CA-04)
- `DELETE /api/v1/users/me/delete` con `{ "password": "..." }` (rechaza miembros desconocidos). El usuario sale del token de acceso.
- **CA-01:** la contraseña se verifica con Argon2id (mismo limitador de hashing que el login) antes de borrar nada; una incorrecta responde **403** (no 401, para que la app no lo confunda con una sesión vencida) con mensaje genérico. **CA-04:** cuota por usuario de **5 intentos en 15 minutos y bloqueo de 15** en Redis Auth (`delete-account`), contada antes de verificar; 429 con `Retry-After`. Una cuenta inexistente y una contraseña incorrecta dan la misma respuesta.
- **CA-02:** `AccountEraser` revoca todas las sesiones y sus access tokens (Redis) lo primero, y al final borra sesiones, refresh tokens y la cuenta.
- **CA-03:** el mismo `AccountEraser` borra los datos (pm-162 y pm-163, ver sus planes) reutilizando los handlers de baja de mascota y de negocio. La cuenta se borra al final: un fallo a mitad deja la cuenta localizable y repetir la eliminación termina el trabajo.
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- 403 por contraseña incorrecta y la cuota de 5 intentos / 15 minutos.
- El borrado es inmediato (el aviso de privacidad de la maqueta promete hasta 30 días por las copias de seguridad: pendiente de confirmar).
- Sin transacciones (Atlas $0): la eliminación no es atómica; el orden descrito la hace reintentable.

## Verificación
- Contrato: `Endpoint=users-me-delete` (`AccountDeletionEndpointsTests`): contraseña incorrecta no borra, la correcta borra todo y deja la sesión inservible, otras cuentas intactas, bloqueo 429, 400 y 401.
