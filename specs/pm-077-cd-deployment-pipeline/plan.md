# Plan — pm-077-cd-deployment-pipeline (solo registro retrospectivo de T089)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- Job `deploy` en `pipeline.yml`: solo en `push` a `main`, tras `quality-gate` y `docker-build`.
- Prueba de humo por HTTPS (`scripts/smoke-test.sh`) con reversa automática a la revisión anterior si falla; DAST con ZAP después.

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
