# Plan — pm-081 private-image-access (T093)

## Enfoque

- `GET /api/v1/pets/get-photo-url/{id}` devuelve una URL temporal de la foto privada y el instante en que caduca.
- La autorización va **antes** de firmar (CA-02): si la mascota no es del propietario del token, responde 404 y no se emite ninguna URL. La firma solo ocurre después de leer la mascota del propietario.
- La URL es una SAS de **solo lectura** que dura **15 minutos**. Con identidad administrada se firma con una *user delegation key*, que caduca sola y no expone la llave de la cuenta.
- El contenedor `pet-photos` no admite acceso anónimo (CA-03): sin SAS vigente, el blob no se puede leer. No existe ninguna URL permanente para la foto de una mascota.
- La ficha del escaneo (pm-127, pm-128) sigue mandando `photoUrl`: cuando la mascota tiene foto subida, ahí también va una URL temporal, emitida solo si la autorización del dueño está vigente.
- Las fotos de negocio son públicas a propósito (pm-111) y no pasan por aquí.

## Pruebas

- De contrato: la URL se emite para el propietario, caduca en 15 minutos, y una mascota ajena responde 404 sin URL.
- Unitarias: una mascota sin foto subida responde 404 (`data: null`).

## Fuera de alcance

- Revocar una URL ya emitida antes de que caduque: la SAS no se puede cancelar y la ventana corta es la mitigación aceptada.
