# Plan — pm-055 ci-pull-request-pipeline (solo T066)

> T067 (ramas protegidas) no forma parte de este plan: cambia la configuración del repositorio en GitHub y requiere autorización expresa.

## Enfoque
- El workflow único `.github/workflows/pipeline.yml` (CI/CD DevSecOps) se dispara en `pull_request` hacia `developer`, `release` y `main`. En pull request ejecuta todas las etapas de calidad e imagen y nunca despliega (`deploy` exige `push` a `main`).
- Paridad con `scripts/quality-gate.ps1` (CA-02): se comprueba paso por paso en `checklists/gates.md`. Umbral único de vulnerabilidades NuGet: High y Critical bloquean, en local y en CI (decisión del usuario, 2026-11-08).
- Herramientas $0: runners de GitHub, gitleaks, Trivy, Syft y acciones oficiales. Sin paquetes nuevos.

## Verificación
- Revisión estática de `pipeline.yml` contra cada paso del gate local.
- Build Release local en el contenedor del SDK.
