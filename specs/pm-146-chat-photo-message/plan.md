# Plan — pm-146-chat-photo-message

## Diseño (pm-146#CA-01 a CA-03)
- `POST /api/v1/messages/send-photo?conversationId=` (multipart, campo `file`) crea un mensaje de tipo `Photo` con la clave del objeto; `GET /api/v1/messages/get-photo-url?conversationId=&messageId=` emite una URL temporal de 15 minutos, siguiendo el patrón de `pets/get-photo-url` (pm-081). Los listados indican `hasPhoto`; el cliente pide la URL solo de esos mensajes.
- **CA-01:** nuevo `ImageContainer.ChatPhotos` con tope de 1 MB; el tipo se deduce de los bytes (`ImageRules`). **CA-02:** la foto vive en el contenedor privado ya existente bajo el prefijo `chat/{conversación}/{mensaje}` (sin recursos nuevos); solo los participantes obtienen URL, el resto recibe 404. **CA-03:** al eliminar una mascota se borran las fotos de sus conversaciones antes de los mensajes (`PhotoKeysByConversations`).
- La foto se guarda antes del mensaje; si el mensaje falla, se borra la foto para no dejar huérfanos.
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Un mensaje con foto no lleva texto (el Spec no pide pie de foto).
- La clave se guarda en el contenedor de mascotas bajo `chat/` en vez de crear un contenedor nuevo (evita tocar la infraestructura).
- El cierre de cuenta (pm-162/pm-163) deberá borrar también estas fotos; hoy solo lo hace la baja de la mascota.

## Verificación
- Contrato: `Endpoint=messages-send-photo` y `messages-get-photo-url`.
