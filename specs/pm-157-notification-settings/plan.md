# Plan — pm-157-notification-settings

## Diseño (pm-157#CA-01 a CA-03)
- Ruta: la convención del proyecto exige acción literal (`RouteCatalogTests`), así que en vez de `GET/PUT /api/v1/notification-settings` del Spec se exponen `GET /api/v1/notification-settings/get` y `PUT /api/v1/notification-settings/update`. **Desviación a confirmar.**
- Un documento por usuario (`_id` = usuario) en `notification_settings`. Sin documento se devuelven los valores por defecto (todo activo, sonido `default`, 3 días).
- PUT reemplaza todo: `vaccines`, `reviews`, `chat`, `followups` (bool), `sound` y `reminderDaysBefore`, todos obligatorios; miembros desconocidos devuelven 400.
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Sonidos permitidos: `default`, `soft`, `silent`.
- Rango de anticipación: 1 a 14 días; por defecto 3 (pm-154#CA-01).
- Sin ajuste de promociones (el Spec lo deja fuera).

## Verificación
- Contrato: `Endpoint=notification-settings-get` y `notification-settings-update`.
