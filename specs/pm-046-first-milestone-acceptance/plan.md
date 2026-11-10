# Plan — pm-046 first-milestone-acceptance (T041)

## Enfoque
- **CA-01:** validación de extremo a extremo de los 11 endpoints del primer hito (4 de autenticación: register, login, refresh y logout; 7 funcionales: perfil `users/me/get` y `users/me/update`, y mascotas `create`, `get-all`, `get-by-id`, `update` y `delete`) más las sondas `health/live` y `health/ready`.
- La validación usa el artefacto que se despliega: la imagen `petmate-api:local` (runtime chiseled), Atlas real y Redis reales vía `docker compose`, y el script reproducible `scripts/acceptance-milestone.ps1` (39 comprobaciones: casos felices, validación 400, 401 sin token, aislamiento entre dos usuarios con 404 en recursos ajenos, borrado físico y revocación en logout).
- Se complementa con las suites completas, la auditoría NuGet, el escaneo de la imagen y la integridad SDD (gates).
- **CA-02:** la aprobación explícita del hito la da el usuario; un agente no la firma.

## Límites de esta validación
Se ejecuta en local (Docker Desktop); la ejecución del pipeline en GitHub y el despliegue en Azure con esta versión son aparte (pm-077 y pm-084).
