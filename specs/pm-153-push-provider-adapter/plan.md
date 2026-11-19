# Plan — pm-153-push-provider-adapter

## Selección y costo (pm-153#CA-04)
- Proveedor: **Firebase Cloud Messaging**, API HTTP v1 (`POST https://fcm.googleapis.com/v1/projects/{id}/messages:send`).
- **Costo $0:** la página de producto y la de planes de Firebase (firebase.google.com/products/cloud-messaging y /docs/projects/billing/firebase-pricing-plans) lo listan sin cargo en los planes Spark y Blaze; verificado el 2026-11-17. Cuota por defecto de 600 000 mensajes por minuto por proyecto, muy por encima del uso previsto.
- **Sin paquetes nuevos:** se descartó el SDK de Google (`FirebaseAdmin`/`Google.Apis.Auth`: dependencias extra y árbol transitivo a auditar). El adaptador usa `HttpClient` y firma el JWT RS256 de la cuenta de servicio con `System.Security.Cryptography.RSA` (biblioteca base). Licencia, vulnerabilidades y compatibilidad con .NET 10: no hay paquete que evaluar.

## Diseño (pm-153#CA-01 a CA-03)
- **CA-01:** puerto `IPushSender` (`Send(token, mensaje)` → `Sent | InvalidToken | Failed | Disabled`) en Application; los casos de uso no conocen a FCM. Adaptadores `FcmPushSender` y `DisabledPushSender` en Infrastructure.
- **CA-02:** la credencial sale solo de `FCM_SERVICE_ACCOUNT_JSON` (JSON completo de la cuenta de servicio, secreto del entorno; `.env.example` solo trae el nombre). `ToString` de las opciones no imprime nada; los logs registran solo el código de estado o el tipo de excepción, nunca la credencial, el token de acceso, el token del dispositivo ni el contenido.
- **CA-03:** `Send` nunca lanza: red caída, 5xx, credencial rechazada, clave mal formada o espera de 5 s devuelven `Failed`, y quien llama sigue con su operación. El token de acceso OAuth se guarda hasta 1 minuto antes de vencer; un 401 lo descarta.
- Un **404 UNREGISTERED** se devuelve como `InvalidToken` para que quien envía borre el token (pm-152#CA-02, lo hará T173).

## Decisiones del agente a confirmar
- Una sola variable con el JSON de la cuenta de servicio (en vez de tres).
- Solo 404 `UNREGISTERED` cuenta como token inválido; un 400 `INVALID_ARGUMENT` se trata como fallo para no borrar tokens por un error de contenido.
- Sin reintentos ni cola: un aviso fallido se pierde (los recordatorios podrán reenviarse desde T173 porque su envío se reclama).

## Verificación
- `FcmPushSenderTests` (SecurityTests) con un proveedor falso y una clave RSA generada en la prueba: contrato del JWT (firma verificada con la clave pública), envío, reutilización del token, `UNREGISTERED`, errores sin excepción, y ausencia de secretos en logs y texto.
- **No se probó contra FCM real** (no hay proyecto Firebase ni credencial): el envío real queda sin verificar.
