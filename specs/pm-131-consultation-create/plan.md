# Plan — pm-131-consultation-create

## Enfoque
- `Consultation` es un registro del carnet (`HealthRecord`) con origen de negocio. `POST consultations/create` pasa por `AccessGate`: cuenta con negocio, tipo `Veterinary` (tienda y estética → 403) y autorización vigente que lo incluya (si no, 403); la mascota se lee con el dueño de la autorización. Motivo y diagnóstico obligatorios (500), peso opcional (0.1–200 kg, un decimal). **Decisión (nota de HU-015):** el peso de la consulta se queda en la consulta y no se suma al historial de peso. Tope de 200 consultas por mascota (409).
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
