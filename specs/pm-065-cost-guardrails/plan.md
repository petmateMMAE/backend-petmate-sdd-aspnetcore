# Plan — pm-065-cost-guardrails (solo registro retrospectivo de T077)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- Presupuesto mensual del grupo de recursos en Terraform (`monitoring.tf`, `enable_budget`, 40 USD por defecto) con avisos al 50, 80 y 100 %.
- Etiquetas comunes `local.tags` en los recursos de `infra/azure`; los grupos de recursos las reciben desde `bootstrap.ps1`.
- Política y estudio de costos: `docs/infrastructure/azure-cost-policy.md` y `azure-cost-study.md`.

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
