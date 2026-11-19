# Plan — pm-075-backend-two-instances (solo registro retrospectivo de T087)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.

## Enfoque encontrado
- `petmate-api` con `min_replicas = 2` y `max_replicas = 2` en modo evaluación; sondas live/ready configuradas.
- La API no guarda estado local: sesiones en Mongo y revocaciones en Redis Auth.

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
