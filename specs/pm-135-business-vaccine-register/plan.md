# Plan — pm-135-business-vaccine-register

## Enfoque
- `POST vaccines/create-by-business`: misma comprobación `AccessGate`; «aplicada por» lo fija el servidor con el nombre del negocio y el contrato rechaza `appliedBy`. Origen de negocio: el dueño no puede modificar ni borrar (403 «lo registró el negocio» de `pm-098`). Próxima dosis posterior a la aplicación.
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
