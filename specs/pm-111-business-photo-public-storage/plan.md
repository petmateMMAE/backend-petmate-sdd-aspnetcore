# Plan — pm-111 business-photo-public-storage (T129)

## Enfoque

- Contenedor **distinto** del de las mascotas (CA-01): `business-photos`, con acceso público de solo lectura a los blobs; `pet-photos` sigue privado. Separar por contenedor y no por cuenta evita un segundo recurso y su costo.
- `Business` gana `PhotoUrl`: la URL pública y permanente del blob. Se devuelve en el perfil público (`businesses/get-by-id`) y en el mapa (`businesses/get-nearby`), que hasta ahora mandaban `null`.
- Límite de **1 MB** y los mismos tipos permitidos que pm-080 (CA-02), con la misma validación por bytes mágicos.
- Solo el afiliado (CA-03): el negocio se resuelve siempre desde el token, como el resto del módulo. Una cuenta sin negocio responde 403.
- El blob se guarda con el identificador del negocio en la ruta, así que reemplazar la foto sobrescribe y nunca deja dos archivos de un mismo negocio. Dar de baja el negocio borra su foto.
- ADR-006 se enmienda (CA-04) con el contenedor público y el costo verificado del Blob.

## Pruebas

- De contrato: subir, reemplazar y borrar la foto del negocio; cuenta sin negocio; archivo mayor de 1 MB; tipo no permitido.
- La foto aparece en el perfil público y en el mapa.

## Fuera de alcance

- CDN (pm-082), que sigue sin aprobarse.
