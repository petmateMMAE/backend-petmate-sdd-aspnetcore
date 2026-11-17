# Plan — pm-167-integration-environment-flow

## Diseño (pm-167#CA-01 a CA-03)
- Un solo documento, `docs/ci-cd/mobile-development-flow.md`, sin cambios de código, de pipeline ni de infraestructura. Resume y enlaza lo ya documentado en `pipeline.md`, `azure-deployment.md` y `version-matrix.md` para no duplicar reglas.
- **CA-01:** diagrama y pasos developer → PR → CI → merge a `main` → CD → prueba de humo → reversa, y la etiqueta solo a mano.
- **CA-02:** entorno compartido (demo de Azure), su estado verificado, contrato (artefacto `openapi` de pm-166) y datos de prueba (no hay datos sembrados).
- **CA-03:** Azure y fusionar a `main` (y la etiqueta) requieren autorización expresa del usuario.

## Verificación
- Estado del entorno: el 2026-11-17 un `GET /api/v1/health/live` a la URL declarada respondió 200 (única petición hecha; no se ejercitó nada más). El Spec indicaba que el 2026-11-09 no respondía; el documento lo deja como estado fechado, no como garantía.
- Es un cambio solo de documentación: `scripts/check-sdd.ps1` no se ejecutó en esta sesión (PowerShell).
