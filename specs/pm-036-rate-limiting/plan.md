# Plan — pm-036 rate-limiting (T054 y T057)

## Enfoque
- **T054 (retrospectiva):** se documenta el límite por IP de autenticación ya implementado (`ConfigureRateLimiting`, `RateLimitKey`, `AUTH_RATE_LIMIT_PER_MINUTE`) con las pruebas existentes; solo se afirma lo que la evidencia demuestra.
- **T057 (CA-03):** matriz de límites para los endpoints no autenticación en `docs/api/endpoints.md`. Se añadió la política por usuario `users` (60 por minuto, `USERS_RATE_LIMIT_PER_MINUTE`) al perfil propio, siguiendo el esquema ya aplicado a `pets`; el resto de decisiones de la matriz está en el spec §6.

## Enmienda 2026-11-08 — T100 (H6)
- Límite por IP distribuido en Redis Auth, validación de proxies al arrancar y documentación de saltos.
- Aprobado por el usuario el 2026-11-08 (opciones A de H7, H3 y H8; B de H4; A, B y C de H6).
