# Plan — pm-137-clinical-history-view

## Enfoque
- `GET history/get-all?petId=&types=` mezcla consultas, vacunas, desparasitaciones y pesos (hasta 200 por tipo) ordenados por fecha descendente e identificador, con filtro `types` (valor inválido → 400) y paginación de `pm-047`. Mascota ajena → 404.
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
