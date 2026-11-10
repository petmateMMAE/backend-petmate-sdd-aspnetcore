# Plan — pm-059 release-versioning

## Enfoque
- Archivo `VERSION` (SemVer) o entrada `version` de `workflow_dispatch`; el job `deploy` rechaza valores que no cumplen `^[0-9]+\.[0-9]+\.[0-9]+$` (CA-01).
- Etiqueta Git `api-vX.Y.Z` creada por el job `deploy` solo tras despliegue y prueba de humo correctos sobre `main`. **Decisión del usuario (2026-11-08, opción 2):** el merge a `main` despliega pero NO etiqueta. La etiqueta solo se crea cuando una persona lanza el flujo a mano (`workflow_dispatch`) y marca la casilla `create_tag`; esa casilla es la autorización expresa. Un pull request nunca etiqueta (CA-02).
- Trazabilidad (CA-03): imagen `petmate-api:X.Y.Z-<sha7>` inmutable en ACR, etiquetas OCI `org.opencontainers.image.version` y `.revision`, variable `PETMATE_VERSION` y sufijo de revisión `vX-Y-Z-<run>`. Detalle en `docs/versioning/version-matrix.md`.
