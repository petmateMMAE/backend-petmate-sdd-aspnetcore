# Plan — pm-152-device-token-unregister

## Diseño
- `POST /api/v1/devices/unregister`, cuerpo `{ "token": "..." }`, con el mismo contrato y validación que el registro. Borra el token solo si es del usuario del token de acceso; uno ajeno o inexistente responde 200 sin cambios (idempotente y sin revelar de quién es un token).
- **CA-01** cubierto. **CA-03** (borrar los tokens al eliminar la cuenta) quedó cubierto: `AccountEraser` llama a `IDeviceRepository.DeleteByUser` (pm-161). **CA-02** (borrar los tokens que el proveedor reporta inválidos) quedó cubierto por el despacho de recordatorios (pm-155/T173): `ReminderDispatcher` borra el token cuando FCM responde `UNREGISTERED`.
- Sin paquetes nuevos.

## Verificación
- Contrato: `Endpoint=devices-unregister` (idempotencia, no borra tokens ajenos, 400, 401).
