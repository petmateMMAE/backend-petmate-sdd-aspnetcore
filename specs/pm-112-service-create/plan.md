# Plan — pm-112-service-create

## Enfoque
- `POST services/create`: nombre obligatorio (1 a 100), descripción (hasta 300) y precio opcionales (≥ 0, 2 decimales, máximo 1 000 000; sin precio es `null`, nunca 0). Sin existencias ni ventas: el contrato rechaza `stock` u otros miembros. Cuenta sin negocio → 403.
- Pruebas: unitarias (dominio y casos de uso), de contrato (con `[Trait("Endpoint", …)]`), de seguridad (negocio siempre el de la cuenta del token) e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`, 1–10000) y 16 KB de cuerpo, como mascotas y carnet.
- Límites de campos de este hito (guardas del agente, a confirmar): nombre 100, dirección 200, descripción 500, cédula 30.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
