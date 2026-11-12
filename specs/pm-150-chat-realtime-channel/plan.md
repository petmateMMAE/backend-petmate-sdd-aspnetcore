# Plan — pm-150-chat-realtime-channel

## Evaluación (pm-150#CA-04)
- Entregar mensajes sin recargar exige una conexión viva (WebSocket o SSE) y, con dos o más instancias, un respaldo compartido (Redis Pub/Sub o similar) que reparta cada mensaje a la instancia donde está conectado el otro participante, sin perder ni duplicar mensajes.
- Costo y complejidad: Redis Cache y Redis Auth son servicios separados con reglas propias (Constitución §5); usar uno de ellos para Pub/Sub o añadir un tercero exige una decisión de la Constitución. Además hay que mantener conexiones largas detrás del balanceador de Azure y validar identidad y pertenencia en cada conexión.
- **Recomendación:** no implementar el tiempo real ahora y aplicar el reemplazo previsto por el propio Spec: la app consulta al abrir la conversación y cada pocos segundos mientras está abierta con `GET messages/get-all?conversationId=&after=<último mensaje>`, que ya existe (`pm-145`) y devuelve solo los posteriores. Los avisos push (`pm-159`) cubrirán al participante con la conversación cerrada.
- **Pendiente de decisión del usuario:** aprobar el reemplazo (el Spec cambia de «tiempo real» a «consulta al abrir») o pedir la implementación del tiempo real con su costo. Mientras tanto T168 queda sin implementar.
