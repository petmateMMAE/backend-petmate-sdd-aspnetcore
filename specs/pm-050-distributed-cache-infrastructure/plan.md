# Plan — pm-050 distributed-cache-infrastructure (T061)

## Enfoque
- `ICacheStore` (Read/Write/Remove), `CacheKey` con la convención `petmate:cache:v1:{área}:{id}`, TTL obligatorio 1 s a 1 h y `RedisCacheStore` sobre `RedisCacheConnection` con tiempo máximo de 250 ms y fallo seguro.
- Sin paquetes nuevos (componentes del SDK de .NET 10 y `coverlet`/xUnit ya centrales; licencias MIT, costo $0) y sin recursos de nube.
