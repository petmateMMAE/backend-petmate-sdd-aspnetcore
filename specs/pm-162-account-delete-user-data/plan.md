# Plan — pm-162-account-delete-user-data

## Diseño (pm-162#CA-01 a CA-03)
- Se implementa dentro de `AccountEraser` (ver pm-161): por cada mascota de la cuenta se ejecuta el mismo `DeleteMyPetHandler` de pm-104 (carnet, QR, autorizaciones, escaneos, conversaciones, mensajes, fotos privadas, consultas de veterinarias y recordatorios), de 50 en 50 hasta que no quedan. Después se borran los tokens de dispositivo, las preferencias de aviso y los tokens de restablecimiento.
- **CA-02:** las consultas registradas por veterinarias en el carnet del dueño se borran con la mascota (`consultations.DeleteByPet`).
- **CA-03:** `AccountDeletionCoverageTests` (seguridad) falla si se agrega una colección a `MongoCollectionNames` sin declarar quién la limpia.
- Sin paquetes nuevos.

## Pendiente
- Las fotos del chat se borran con la mascota (pm-146); no hay otro lugar donde un usuario tenga archivos.
- Sin transacciones: un fallo a mitad se corrige repitiendo la eliminación.

## Verificación
- Contrato `users-me-delete` (mascota, vacuna, recordatorio, dispositivo y preferencias desaparecen) y `AccountDeletionCoverageTests`.
