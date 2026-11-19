# Plan — pm-068-cloud-secrets-configuration (solo registro retrospectivo de T080)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- Secretos como secretos de Container Apps (`mongodb-uri`, `jwt-signing-key`, conexiones Redis) alimentados con `TF_VAR_*` desde secretos de GitHub.
- La identidad de GitHub Actions usa OIDC (sin credenciales de larga duración).
- Key Vault no se usa (ADR-010 lo difiere por costo).

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
