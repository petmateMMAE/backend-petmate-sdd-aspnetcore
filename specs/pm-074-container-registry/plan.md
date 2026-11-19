# Plan — pm-074-container-registry (solo registro retrospectivo de T086)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- ACR Basic (`admin_enabled = false`), acceso por identidad administrada con rol `AcrPull`.
- El job `deploy` publica la imagen con etiqueta `{VERSION}-{sha}` y la bloquea (`--write-enabled false --delete-enabled false`).

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
