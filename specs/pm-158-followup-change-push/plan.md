# Plan — pm-158-followup-change-push

## Diseño (pm-158#CA-01 a CA-03)
- `UpdateFollowupHandler` compara el estado y la próxima revisión antes y después; **solo un cambio real** avisa al dueño por push (`PushNotifier`, tema `Followups`): título «`<mascota>`: seguimiento» y texto «Estado: Mejorando. Próxima revisión: dd/MM/yyyy.» (CA-01). Repetir la misma actualización no envía nada (CA-03).
- **CA-02:** `PushNotifier` solo envía si el dueño tiene activo `followups` (pm-157); envía a todos sus dispositivos y borra los tokens que el proveedor reporta inválidos.
- Reabrir la conversación (pm-147) también cambia el estado a «En tratamiento» y avisa al dueño.
- El aviso es inmediato en la propia petición, sin cola ni reintentos (decisión del usuario del 2026-11-17): si FCM falla, el aviso se pierde y la operación sigue (comentario `shortcut:` en `PushNotifier`).
- Sin paquetes nuevos.

## Verificación
- Contrato: un cambio de estado y uno de revisión avisan una vez, repetir no avisa, y con `followups` apagado no se envía.
