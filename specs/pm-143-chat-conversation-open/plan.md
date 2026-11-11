# Plan — pm-143-chat-conversation-open

## Enfoque
- `Conversation` se crea al abrir el seguimiento, con exactamente dos participantes (la cuenta del dueño y la cuenta dueña de la veterinaria), índice único por seguimiento; cualquier otro usuario recibe 404. Los mensajes llevan una secuencia consecutiva asignada de forma atómica con `$inc` en la conversación.
- Pruebas: unitarias (dominio), de contrato (con `[Trait("Endpoint", …)]`), de seguridad e integración contra la base de pruebas de Atlas.
- Contratos: `docs/api/endpoints.md` y `docs/testing/endpoints.md` actualizados.
- Límites: 60 solicitudes/min por usuario (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- Guardas de este bloque a confirmar: máximo 10 medicamentos por receta, 200 consultas por mascota, textos de 500 (consulta y nota) y 1000 (mensaje) caracteres, 200 notas por seguimiento.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
