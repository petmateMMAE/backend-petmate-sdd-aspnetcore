# Plan — pm-057 container-image-scan

## Enfoque
- Trivy (Aqua Security, Apache-2.0, $0) escanea la imagen en el job `6 · Imagen, Trivy y SBOM` (pull request y `main`) y otra vez en `7 · Despliegue` sobre la imagen que se publica.
- Severidades `CRITICAL,HIGH` con `exit-code: 1`: un hallazgo bloquea el job.

## Decisión (2026-11-08, pedida por el usuario)
- Se elimina `ignore-unfixed` en los dos escaneos: cualquier HIGH o CRITICAL bloquea, tenga o no parche. No se usa `.trivyignore`.
- Para que la imagen no arrastre paquetes vulnerables, la etapa final pasa de `dotnet/aspnet:10.0` (Ubuntu 24.04 completo con `apt-get upgrade`) a `dotnet/aspnet:10.0-noble-chiseled`, la imagen "distroless" oficial de Microsoft: sin shell, sin gestor de paquetes, usuario no root (UID 1654) y globalización invariante (la API no usa culturas ni zonas horarias). Licencia MIT y costo $0.
- La CA de Redis del CD se agrega al almacén en una etapa `certs` (con shell) y se copia el `ca-certificates.crt` resultante a la imagen final.
- Consecuencia: `docker-compose.yml` ya no tiene healthcheck de Docker para la API (requería `bash`); la salud se comprueba por HTTP, como en las sondas de Azure Container Apps.
