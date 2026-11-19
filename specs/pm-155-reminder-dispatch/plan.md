# Plan — pm-155-reminder-dispatch

## Diseño (pm-155#CA-01 a CA-04)
- **CA-01:** `ReminderDispatchHostedService` (un `BackgroundService` por instancia) ejecuta `ReminderDispatcher.RunOnce` cada **5 minutos**: envía por `IPushSender` (pm-153) a todos los dispositivos del dueño (pm-151) los recordatorios (pm-154) cuya fecha de aviso (`remindOn`) ya llegó y cuya fecha límite no ha pasado. Tope de 200 por pasada. Sin proveedor push configurado el trabajo no corre y no reclama nada (no se pierde ningún aviso).
- **CA-02:** cada recordatorio se **reclama de forma atómica** (`FindOneAndUpdate` sobre `sentAtUtc` vacío → marca el envío) antes de enviarse; con dos o más instancias solo una lo recibe. Prueba con 8 ejecuciones simultáneas.
- **CA-03:** se envía solo si el dueño tiene activo el aviso de vacunas y desparasitaciones (pm-157, por defecto activo); desactivado, el recordatorio se marca como procesado (sigue en el listado) y no se envía.
- **CA-04:** título «`<mascota>`: vacuna|desparasitación» y texto «`<nombre del registro>`: dd/MM/yyyy».
- Un token que el proveedor reporta inválido (404 `UNREGISTERED`) se borra (cubre pm-152#CA-02). Si el envío falla en todos los dispositivos, el recordatorio vuelve a pendiente y se reintenta en la siguiente pasada (la pasada actual se corta para no reclamarlo en bucle). Si el dueño no tiene dispositivos, el recordatorio queda procesado.
- Índice nuevo `ix_reminders_remindOn_sentAtUtc`. Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Intervalo de 5 minutos y tope de 200 por pasada (el Spec no los fija).
- Las revisiones de seguimiento no se despachan aquí (pm-160).
- Solo el aviso de «vacunas y desparasitaciones» gobierna este envío; `reviews`, `chat` y `followups` serán de pm-158 a pm-160.

## Riesgos
- **Modo ahorro de Azure:** si la API escala a cero tras 15 minutos sin tráfico (`MODO_AHORRO`), el trabajo no corre mientras está apagada y los avisos salen cuando despierte. No se tocó nada de Azure; hay que decidirlo al desplegar.
- Atajo (`shortcut:` en el código): el envío se marca al reclamar; si la instancia cae entre reclamar y enviar, ese aviso se pierde sin reintento.
- No hay prueba contra Atlas del reclamo atómico ni contra FCM real.

## Verificación
- `ReminderDispatcherTests` (unitarias, 8 casos): envío único a todos los dispositivos con el texto esperado, fecha de aviso futura, concurrencia, aviso desactivado, token inválido borrado, reintento tras fallo total, sin proveedor y mascota inexistente.
