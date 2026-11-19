# Plan — pm-160-review-reminder-push

## Diseño (pm-160#CA-01 a CA-03)
- La próxima revisión de un seguimiento es un recordatorio más (`ReminderKind.Review`, mismo despacho de pm-155) con **un día de anticipación fijo** (no la preferencia de días de pm-157). Se programa al abrir el seguimiento y al actualizarlo.
- **CA-01:** el despacho lo envía un día antes al dueño si tiene activo el aviso de **revisiones** (`reviews` de pm-157); título «`<mascota>`: revisión».
- **CA-02:** al cambiar la fecha el recordatorio se reprograma (y, si ya se había enviado, vuelve a pendiente para la fecha nueva); al dar de alta (sin revisión) se cancela. También se borra con la mascota.
- **CA-03:** idempotente por construcción (clave = seguimiento; un envío reclamado no se repite).
- Esto completa la parte de «revisiones» de pm-156#CA-01 (el listado incluye los recordatorios de revisión).
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Se usa el aviso `reviews` (no `followups`) porque pm-157 los separa: «revisiones» frente a «cambios del seguimiento».
- Una revisión cuya fecha es hoy o mañana se avisa en la siguiente pasada del despacho (el aviso de «un día antes» puede salir con menos de un día de margen).

## Verificación
- Contrato (`VeterinaryEndpointsTests`): recordatorio a 1 día, reprogramado y cancelado al dar de alta. Unitaria: el despacho usa `reviews` y dice «revisión».
