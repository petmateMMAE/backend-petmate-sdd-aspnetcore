# Plan — pm-159-chat-message-push

## Diseño (pm-159#CA-01 a CA-03)
- Al enviar un mensaje de texto o una foto, `SendMessageHandler` y `SendPhotoMessageHandler` avisan al **otro** participante (`PushNotifier`, tema `Chat`) con «Nuevo mensaje» y un texto genérico («Tu veterinaria te escribió…» / «Un cliente te escribió…»). Una sola vez por mensaje (CA-03). No incluye el contenido del mensaje ni de la foto, así que nada clínico sale hacia el proveedor push (CA-02).
- **CA-01, «salvo que haya desactivado ese aviso»:** respeta el ajuste `chat` de pm-157.
- **Supresión mientras se lee (decisión del usuario del 2026-11-17):** sin tiempo real (pm-150 se resolverá consultando), «está leyendo» significa que la persona consultó la conversación (`messages/get-all` o `conversations/mark-read`) hace menos de **30 segundos**. Cada consulta marca `ownerSeenAtUtc`/`businessSeenAtUtc` en la conversación en Atlas (a lo más una escritura cada 10 s); si el destinatario la consultó dentro de la ventana, no se le avisa. No se usó la caché Redis: la política de pm-051 prohíbe guardar datos de conversaciones ahí.
- Aviso inmediato en la petición, sin cola ni reintentos (decisión del usuario).
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Ventana de 30 segundos y refresco de la marca cada 10 segundos (dependen de que la app consulte cada pocos segundos mientras la conversación está abierta; si consulta menos seguido, quien lee recibirá avisos).
- Una app que no consulte (en segundo plano) recibe el aviso, que es lo esperado.

## Verificación
- Contrato: aviso único sin el contenido, ninguno si el destinatario consultó la conversación, ninguno con `chat` apagado, y también para fotos.
