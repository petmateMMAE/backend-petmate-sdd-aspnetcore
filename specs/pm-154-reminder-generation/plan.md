# Plan — pm-154-reminder-generation

## Diseño (pm-154#CA-01 a CA-03)
- Colección `reminders` con el registro de origen (vacuna o desparasitación) como `_id`: un registro tiene a lo más un recordatorio, así que generarlo otra vez no lo duplica (CA-03). El `Upsert` conserva el estado de lectura y de envío.
- `ReminderScheduler` programa el recordatorio al registrar una vacuna (`nextDoseOn`) o una desparasitación (`nextOn`), por el dueño o por una veterinaria, con la anticipación de las preferencias del dueño (pm-157) o **3 días** por defecto (CA-01). Sin próxima fecha se cancela; una fecha ya pasada no genera nada.
- Al borrar el registro (solo el dueño puede borrar los suyos) se cancela (CA-02); al eliminar la mascota se borran todos sus recordatorios.
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Hoy los registros del carnet no se pueden editar (solo crear y borrar), así que «cambiar la fecha» se cubre al volver a programar con el mismo registro; no existe aún un flujo de edición.
- Programar es de mejor esfuerzo: si Atlas no responde, el registro del carnet queda guardado sin recordatorio y no hay reparación automática (comentario `shortcut:` en el código).
- Cambiar la anticipación en los ajustes (pm-157) no reprograma los recordatorios ya creados.
- Las **revisiones de seguimiento** no generan recordatorios aquí: se cubrirán con pm-160 (T178).

## Verificación
- Unitarias: `ReminderSchedulerTests`. Contrato: `ReminderEndpointsTests` (alta, borrado y cascada).
