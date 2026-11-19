# Plan — pm-069-atlas-cloud-integration (solo registro retrospectivo de T081)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- `infra/atlas` agrega las IPs de salida de la API a la lista de acceso del proyecto Atlas; `atlas-ip-sync.yml` las vuelve a sincronizar cada 6 h.
- Usuario de aplicación con `readWrite` solo en `petmate_prod` (procedimiento de `azure-deployment.md` §Atlas).

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
