# Plan — pm-049 json-serialization-performance (T060)

## Enfoque
- Metadatos de serialización generados en compilación (`ApiJsonContext`) para los tipos del wrapper, con respaldo por reflexión; sin paquetes nuevos.
- Sin paquetes nuevos (componentes del SDK de .NET 10 y `coverlet`/xUnit ya centrales; licencias MIT, costo $0) y sin recursos de nube.
