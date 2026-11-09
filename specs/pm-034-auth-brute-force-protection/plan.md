# Plan — pm-034 auth-brute-force-protection (T056)

## Enfoque
- `IAuthAttemptThrottle` (Application) con implementación `RedisAuthAttemptThrottle` en Redis de autenticación: contador con TTL de la ventana y bloqueo con TTL, ambos por hash SHA-256 del sujeto; el conteo es atómico (script Lua). Réplicas distintas comparten el estado.
- `LoginHandler`: consulta el bloqueo del correo antes de buscar la cuenta o calcular hashes (idéntico para correos existentes e inexistentes); registra los fallos; un login correcto borra el contador. `RefreshHandler`: cuota por usuario antes de consumir el token.
- `IPasswordHashingLimiter` (`BoundedPasswordHashingLimiter`): hashes Argon2id fuera del hilo de la solicitud, con tantos simultáneos como núcleos y el doble en cola; el exceso se rechaza al instante.
- `AuthResult` gana `AuthError.TooManyRequests` y `RetryAfter`; `AuthController` responde 429 con el Response Wrapper y la cabecera `Retry-After`.
- Valores en `AuthThrottleRules` (spec §6). Sin paquetes nuevos (StackExchange.Redis ya estaba).

## Verificación
Pruebas unitarias (`ThrottlingTests`), de contrato (`AuthThrottlingEndpointsTests`), de integración con Redis real (`AuthAttemptThrottleTests`) y comprobación con Kestrel y Redis reales.

## Enmienda 2026-11-08 — T101 (H3)
- Bloqueo de login por par (correo, IP) y techo por correo con espera corta.
- Aprobado por el usuario el 2026-11-08 (opciones A de H7, H3 y H8; B de H4; A, B y C de H6).
