# Plan — pm-164-account-deletion-web-request

## Diseño (pm-164#CA-01 a CA-03)
- `POST /api/v1/account-deletion/request` con `{ "email": "..." }` (público). Responde 200 con el mismo mensaje exista o no el correo (CA-01), igual que `forgot-password`.
- Para una cuenta existente se guarda el hash de un token aleatorio de un solo uso que vence a la hora (CA-02) en la colección propia `account_deletion_tokens` (nunca comparte token con el restablecimiento de contraseña) y se envía el enlace con el puerto de correo de pm-088 (`SendAccountDeletion`). Solo el enlace más reciente sirve.
- **CA-03:** límite por IP (filtro de las rutas de autenticación) y cuota por correo de 3 solicitudes en 15 minutos con bloqueo de 15 (cuenta exista o no), 429 con `Retry-After`.
- Correo: sin paquetes nuevos (Resend ya aprobado en pm-088). Variable opcional nueva `ACCOUNT_DELETION_URL_BASE` (HTTPS; se le añade `?token=`); sin ella ese correo no se envía.

## Decisiones del agente a confirmar
- Método de verificación: enlace al correo (la propuesta del Spec, pendiente de confirmar por el usuario).
- Misma cuota que el restablecimiento: 3 solicitudes por correo cada 15 minutos.
- Hoy no hay dominio de envío verificado, así que el envío real no está operativo (igual que pm-088).

## Verificación
- Contrato: `Endpoint=account-deletion-request` (misma respuesta con correo conocido y desconocido, 400, 429, fallo del proveedor no revelado).
