# Plan — pm-156-reminder-list

## Diseño (pm-156#CA-01 a CA-03)
- `GET /api/v1/reminders/get-all?page=&pageSize=` lista los recordatorios del dueño con fecha de hoy en adelante, el más cercano primero, paginado con la convención de pm-047. El nombre de la mascota se resuelve al listar; si la mascota ya no es del dueño, el recordatorio no se muestra (CA-03).
- `isRead` refleja el estado de lectura guardado (CA-02).
- Sin paquetes nuevos.

## Pendientes de decisión (T174 no puede cerrarse)
- **Revisiones (CA-01):** el listado incluye vacunas y desparasitaciones; las revisiones de seguimiento todavía no generan recordatorios (ver pm-154).
- **Estado de lectura (CA-02):** el Spec no define cómo se marca un recordatorio como leído (no hay endpoint). `isRead` siempre es `false` hasta decidirlo.

## Verificación
- Contrato: `Endpoint=reminders-get-all`.
