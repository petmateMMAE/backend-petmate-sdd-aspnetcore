# Plan — pm-058 sbom-generation

## Enfoque
- `anchore/sbom-action` (Syft, Apache-2.0, $0) genera un SBOM SPDX-JSON de la imagen en el job `6 · Imagen, Trivy y SBOM`, en cada ejecución del pipeline (pull request y `main`).
- El SBOM se sube como artefacto `sbom-petmate-api-<sha>.spdx.json` (CA-02).
