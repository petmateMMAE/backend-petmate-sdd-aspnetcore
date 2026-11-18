# Plan — pm-066-iac-baseline (solo registro retrospectivo de T078)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- Terraform elegido (ADR-004) con módulos `infra/azure` e `infra/atlas` y proveedores fijados con lock files.
- `infra.yml` ofrece `plan`, `apply` y `destroy` por `workflow_dispatch`, solo desde `main`.

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
