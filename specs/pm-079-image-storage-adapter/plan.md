# Plan — pm-079 image-storage-adapter (T091)

## Enfoque

- Puerto `IImageStorage` en `PetMate.Application/Images`: guardar, borrar y emitir una URL temporal. La capa de aplicación no conoce Azure.
- Adaptador `BlobImageStorage` en `PetMate.Infrastructure/Images` sobre `Azure.Storage.Blobs`. Dos contenedores con responsabilidades distintas:
  - `pet-photos`: privado. Se lee solo con URL temporal firmada (pm-081).
  - `business-photos`: público de solo lectura (pm-111), nunca lleva datos de una mascota.
- La cuenta y las credenciales llegan por configuración, igual que Mongo y Redis. Dos formas de autenticarse, por orden:
  1. Identidad administrada (`IMAGES_STORAGE_ACCOUNT`), que es como corre en Azure. La firma temporal usa una *user delegation key*.
  2. Cadena de conexión (`IMAGES_STORAGE_CONNECTION`), para desarrollo local contra Azurite.
- Sin configuración se registra `DisabledImageStorage`, que responde que el almacenamiento no está disponible. Es el mismo patrón que `DisabledEmailSender` de pm-088: el proyecto arranca y las pruebas corren sin tocar Azure.
- Nada se escribe en el disco del contenedor (CA-02): el cuerpo de la petición se transmite directo al Blob.

## Paquetes nuevos

- `Azure.Storage.Blobs` y `Azure.Identity`, centralizados en `Directory.Packages.props`.

## Infraestructura

- `infra/azure/storage.tf`: cuenta de almacenamiento de imágenes con TLS 1.2 mínimo, sin acceso anónimo a la cuenta, y los dos contenedores. Roles `Storage Blob Data Contributor` y `Storage Blob Delegator` para la identidad administrada de la API.
- El `apply` lo ejecuta una persona con autorización expresa; el agente nunca crea recursos (`AGENTS.md` §4, Constitución §8.6).

## Fuera de alcance

- CDN (pm-082): sigue sin aprobarse. Las imágenes públicas se sirven desde el Blob, como previó ADR-007.
