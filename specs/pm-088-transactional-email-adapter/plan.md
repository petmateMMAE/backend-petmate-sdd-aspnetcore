# Plan — pm-088-transactional-email-adapter

## Enfoque
- Puerto `IEmailSender` (Application) y adaptador `ResendEmailSender` por HTTP, sin paquetes nuevos. Proveedor: Resend, plan gratuito de 3 000 correos al mes y 100 al día (verificado el 2026-11-09 en resend.com/pricing); exige un dominio de envío verificado (SPF y DKIM), que hoy no existe. Credencial en `EMAIL_API_KEY`, remitente `EMAIL_FROM`, enlace `PASSWORD_RESET_URL_BASE` (HTTPS). Sin las tres variables, `DisabledEmailSender` no envía nada. Espera máxima 5 s; los fallos devuelven `false` y se registran sin credencial, destinatario ni enlace.
- Pruebas: unitarias, de contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
