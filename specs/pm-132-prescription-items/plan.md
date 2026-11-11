# Plan — pm-132-prescription-items

## Enfoque
- La receta va dentro de la consulta: hasta **10 medicamentos** (nombre, dosis, frecuencia y duración obligatorios, indicaciones opcionales, 100 y 300 caracteres). Se envía al crear la consulta y `POST consultations/add-medication` agrega uno después; solo la veterinaria autora (filtro por `businessId` de origen). El dueño no tiene ninguna ruta de escritura.
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
