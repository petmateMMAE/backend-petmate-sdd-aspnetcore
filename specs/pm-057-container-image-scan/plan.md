# Plan — pm-057 container-image-scan

## Enfoque
- Trivy (Aqua Security, Apache-2.0, $0) escanea la imagen en el job `6 · Imagen, Trivy y SBOM` (pull request y `main`) y otra vez en `7 · Despliegue` sobre la imagen que se publica.
- Severidades `CRITICAL,HIGH` con `exit-code: 1`: un hallazgo bloquea el job.

## Decisión pendiente (usuario)
- Ambos pasos usan `ignore-unfixed: true`: un HIGH/CRITICAL sin parche publicado no bloquea. AGENTS.md §6 y pm-055 CA-04 no aceptan ignorar hallazgos sin aprobación. Opciones: (a) aprobar la excepción y registrarla aquí con fecha y motivo; (b) quitar `ignore-unfixed` y gestionar cada CVE sin parche con un `.trivyignore` aprobado.
