# Plan — pm-149-chat-read-state

## Enfoque
- `POST conversations/mark-read`: marca como leído hasta un mensaje dado (o todo) solo el lado de quien lo pide, con `$max` para que nunca retroceda; el otro participante no cambia. El listado informa los sin leer (mensajes posteriores a la lectura que no envió el propio usuario).
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
