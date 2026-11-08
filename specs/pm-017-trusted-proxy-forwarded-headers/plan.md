# Plan — pm-017 trusted-proxy-forwarded-headers (T047, T097)

> Tasks retrospectivas: documentan evidencia que ya existía y solo afirman lo que esa evidencia demuestra.

## Enfoque
- T047 documenta `ConfigureForwardedHeaders`, `UseForwardedHeaders` condicional y `RateLimitKey`. T097 añade la evidencia directa contra la falsificación de `X-Forwarded-For` con `ForwardedHeadersSpoofingTests`, que fija una IP de origen real (el servidor de pruebas no trae una).
- Sin paquetes nuevos ni cambios de contrato.
