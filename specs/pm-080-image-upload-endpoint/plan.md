# Plan — pm-080 image-upload-endpoint (T092)

## Enfoque

- Endpoints nuevos, con los verbos del catálogo (pm-007):
  - `POST /api/v1/pets/upload-photo/{id}` — sube o reemplaza la foto de una mascota propia.
  - `DELETE /api/v1/pets/delete-photo/{id}` — la quita.
  - `POST /api/v1/businesses/upload-photo` y `DELETE /api/v1/businesses/delete-photo` — la del negocio de la cuenta (pm-111).
- Cuerpo `multipart/form-data` con un solo campo `file`. Es la única parte de la API que no es JSON.
- Contrato (CA-01): tipos permitidos **JPEG, PNG y WebP**; tamaño máximo **2 MB** para una mascota y **1 MB** para un negocio (pm-111#CA-02).
- Validación del contenido (CA-02): el tipo sale de los **bytes mágicos** del archivo, no del nombre ni del `Content-Type` que manda el cliente. Un `.jpg` que en realidad es otra cosa se rechaza con 400. La extensión guardada la decide el servidor a partir del tipo detectado.
- Propiedad (CA-03): el identificador del propietario sale siempre del token. Una mascota ajena o inexistente responde **404**, igual que el resto del módulo, para no revelar cuál de las dos es. Un negocio ausente responde **403**, como el resto de las rutas de negocio.
- Límite de cuerpo: `RequestBodyLimitMiddleware` sube de 16 KB a 2 MB solo en estas rutas; el resto de la API no cambia.
- La mascota guarda la **clave del objeto**, no una URL: la foto es privada y su URL se pide aparte (pm-081). El negocio guarda la URL pública directa.
- Al reemplazar o borrar se elimina el blob anterior. Si el borrado del blob falla, la operación continúa: el registro manda y un blob huérfano no rompe nada.

## Pruebas

- Unitarias de la detección de tipo y de los límites de tamaño.
- De contrato: subida válida, tipo no permitido, archivo con extensión mentirosa, archivo demasiado grande, mascota ajena, cuenta sin negocio y sin token.

## Fuera de alcance

- Moderación de contenido y reescalado de imágenes.
