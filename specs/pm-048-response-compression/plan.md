# Plan — pm-048 response-compression (T059)

## Enfoque
- Compresión HTTP gzip/Brotli del JSON, también sobre HTTPS, excluyendo `/api/v1/auth/*` (`SensitiveAwareCompressionProvider`). Riesgos en `docs/security/compression-risk.md`.
- Sin paquetes nuevos (componentes del SDK de .NET 10 y `coverlet`/xUnit ya centrales; licencias MIT, costo $0) y sin recursos de nube.
