# Evaluación de riesgos de la compresión HTTP (pm-048 / T059)

> Fecha: 2026-11-08. Implementación: `ConfigureCompression` en `ApplicationBootstrap` y `SensitiveAwareCompressionProvider`.

## Decisión
- Se comprimen (gzip y Brotli, nivel más rápido) las respuestas `application/json` cuando el cliente envía `Accept-Encoding`; también sobre HTTPS, porque el beneficio es grande (50 mascotas: 12 777 B → 1 909 B con gzip, 14.9 %; 1 901 B con Brotli).
- **Se excluyen todas las rutas `/api/v1/auth/*`**, cuyas respuestas llevan access y refresh tokens.

## Riesgos evaluados
| Riesgo | Evaluación | Mitigación |
|---|---|---|
| BREACH/CRIME (secreto + texto controlado por el atacante en la misma respuesta comprimida) | Aplica a respuestas HTTPS con secretos que reflejen entrada del atacante. Los únicos secretos de la API son los tokens, que solo salen de `/auth/*` | Esas rutas nunca se comprimen (`Authentication_responses_with_tokens_are_never_compressed`); las demás respuestas no incluyen secretos ni reflejan entrada sin sanear |
| Datos personales del propietario en respuestas comprimidas | El perfil y las mascotas son personales, pero no contienen secretos de sesión; el atacante tendría que adivinarlos | Autenticación obligatoria por token en `Authorization` (no en cookie), por lo que no hay CSRF para forzar solicitudes cruzadas |
| Costo de CPU / DoS | Comprimir cuesta CPU | Nivel `Fastest`; cuerpo de petición limitado a 16 KB; paginación máxima de 50 elementos |
| Caché intermedia | Respuestas distintas por codificación | El middleware añade `Vary: Accept-Encoding` |

## Pruebas y límites
`CompressionAndJsonTests` verifica la compresión solicitada, el contenido idéntico al descomprimir, `Vary` y que registro, login y login fallido no se comprimen. Limitaciones: la medición es sobre datos sintéticos; si en el futuro una ruta fuera de `/auth/*` devuelve un secreto, debe añadirse a la exclusión.
