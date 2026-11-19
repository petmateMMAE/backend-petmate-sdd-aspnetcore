# Plan — pm-151-device-token-register

## Diseño (pm-151#CA-01 a CA-03)
- `POST /api/v1/devices/register`, cuerpo `{ "token": "..." }`, cualquier cuenta autenticada; el usuario sale solo del claim `sub`. Rechaza miembros desconocidos. Reutiliza la política de límite por usuario `users` (60 por minuto).
- Colección `devices` con el token como `_id` (único): un registro repetido no duplica (CA-02) y un token que venía de otra cuenta pasa a la actual (el dispositivo cambió de sesión). Índice `ix_devices_userId` para el conteo.
- Máximo **5 dispositivos por usuario**; un token nuevo por encima del máximo responde 409. Registrar un token ya propio siempre es 200 (idempotente).
- Token de 20 a 4096 caracteres, sin espacios ni caracteres de control. `ToString` del comando y de la petición no lo imprime y el mensaje de validación no lo repite (CA-03).
- Sin paquetes nuevos: costo $0 y sin dependencias que auditar.

## Decisiones del agente a confirmar
- Máximo de 5 dispositivos y 409 al superarlo (el Spec no fija el número ni la respuesta).
- Atajo: contar y escribir no es atómico; dos registros simultáneos podrían pasar el máximo por uno. No es un control de seguridad.
- Sin campo de plataforma ni nombre del dispositivo: el Spec solo pide el token.

## Verificación
- Contrato: `Endpoint=devices-register` (`DeviceEndpointsTests`); seguridad: `DeviceTokenSecrecyTests`.
- Sin prueba de integración contra Atlas (la escritura contra el repositorio Mongo queda sin ejecutar; Atlas no responde desde este equipo según `Memoria.md`).
