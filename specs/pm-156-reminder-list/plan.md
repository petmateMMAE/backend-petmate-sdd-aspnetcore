# Plan — pm-156-reminder-list

## Diseño (pm-156#CA-01 a CA-03)
- `GET /api/v1/reminders/get-all?page=&pageSize=` lista los recordatorios del dueño con fecha de hoy en adelante, el más cercano primero, paginado con la convención de pm-047. El nombre de la mascota se resuelve al listar; si la mascota ya no es del dueño, el recordatorio no se muestra (CA-03).
- `isRead` refleja el estado de lectura guardado (CA-02).
- Sin paquetes nuevos.

## Estado de lectura y revisiones (2026-11-17)
- **Revisiones (CA-01):** ya entran en el listado porque pm-160 las programa como recordatorios `Review`.
- **Estado de lectura (CA-02):** decisión del usuario: una marca por aviso. `POST /api/v1/reminders/mark-read` `{ "id" }` guarda `readAtUtc` del recordatorio del dueño; idempotente; ajeno o inexistente → 404; `id` vacío o miembros desconocidos → 400.

## Verificación

- Contrato: `Endpoint=reminders-get-all`.
