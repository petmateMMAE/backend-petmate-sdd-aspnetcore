# Plan — pm-067-iac-state-management (solo registro retrospectivo de T079)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- Backend `azurerm` con autenticación Azure AD (`use_azuread_auth`, sin claves de cuenta) y estado en `rg-petmate-tfstate`, fuera del grupo de la app.
- `bootstrap.ps1` crea la cuenta con TLS 1.2, sin acceso público a blobs, sin claves compartidas y con versionado.
- `infra.yml` usa `concurrency: infra-production` para no ejecutar dos aplicaciones a la vez.

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
