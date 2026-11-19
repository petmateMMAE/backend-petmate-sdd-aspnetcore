# Plan — pm-082-cdn-public-delivery (solo registro retrospectivo de T094)

> Registro de lo que **ya existe** en el repositorio (2026-11-18). No es un plan de trabajo nuevo ni autoriza crear recursos Azure.
> **El Spec sigue sin aprobación humana**: este documento solo registra evidencia y no autoriza implementar ni planificar nada más.

## Enfoque encontrado
- No hay CDN. ADR-007 lo difiere ("Diferida") y propone servir con Blob y URLs temporales (pm-081).
- pm-079 creó el contenedor público de fotos de negocios, que es el origen candidato.

## Costo, licencia y dependencias
- Sin paquetes ni acciones nuevas registrados aquí. Costos: [`azure-cost-policy.md`](../../docs/infrastructure/azure-cost-policy.md) (crédito de Azure for Students, verificar vigencia antes de aplicar).

## Verificación
- Revisión estática de `infra/`, `.github/workflows/` y documentos citados; resultado por criterio en [checklists/gates.md](checklists/gates.md).
- No se ejecutó `terraform plan` ni se consultó Azure o Atlas.
