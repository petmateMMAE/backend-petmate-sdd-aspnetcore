# Plan — pm-138-followup-open

## Enfoque
- `Followup` (uno por consulta, índice único `ux_followups_consultationId`; 409 si ya existe), estado `Treating`/`Improving`/`Discharged`, próxima revisión de 3, 7 o 15 días (se guarda como fecha). Solo la veterinaria autora de la consulta lo abre (consulta ajena → 404). Guarda el nombre de la mascota al abrir para el listado.
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
