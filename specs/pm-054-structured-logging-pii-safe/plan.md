# Plan — pm-054 structured-logging-pii-safe (T065)

## Enfoque
- Sin paquetes nuevos: `Microsoft.Extensions.Logging.Console` con formateador JSON (incluido en el SDK; licencia MIT, costo $0).
- `RequestLoggingMiddleware` (primero en la tubería): asigna un identificador de correlación (acepta `X-Correlation-Id` solo si es `[A-Za-z0-9-]{8,64}`, si no genera uno), lo devuelve en `X-Correlation-Id` al iniciar la respuesta (el Response Wrapper limpia las cabeceras antes de escribir el cuerpo), lo publica como ámbito `CorrelationId` y registra un evento por solicitud con método, **plantilla** de ruta, código HTTP y duración.
- Nunca se registran cuerpos, cabeceras, consulta, IP, valores de ruta ni identidad del usuario.
- Se filtra `Microsoft.AspNetCore.Hosting.Diagnostics` a `None`: su ámbito `RequestPath` incluye identificadores y texto arbitrario del cliente (hallado por las pruebas).
- Registro estructurado de los demás eventos (excepciones saneadas, Atlas/Redis no disponibles) ya existente con `LoggerMessage`.

## Fuera de alcance
- Eventos de seguridad (login fallido, bloqueo, replay de refresh): requieren decidir qué identificador pseudónimo se registra (hallazgo I2) y quedan como decisión pendiente.
- Destino de los logs en Azure: pm-078.
