# Plan — pm-106-business-schedule-model

## Enfoque
- `WeeklySchedule`: por día, hasta 6 franjas `HH:mm` con cierre posterior a la apertura y sin solapes; día sin franjas = cerrado; `IsOpenAt` convierte el instante a America/Mexico_City (si el sistema no tiene la base de zonas usa UTC-6 fijo, México no usa horario de verano). El cierre no cuenta como abierto.
- Pruebas: unitarias (dominio y casos de uso), de contrato (con `[Trait("Endpoint", …)]`), de seguridad (negocio siempre el de la cuenta del token) e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`, 1–10000) y 16 KB de cuerpo, como mascotas y carnet.
- Límites de campos de este hito (guardas del agente, a confirmar): nombre 100, dirección 200, descripción 500, cédula 30.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
