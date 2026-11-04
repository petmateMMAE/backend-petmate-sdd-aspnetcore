# Fuentes y alcance de esta preparación

## Fuentes utilizadas

1. Archivo `SDD_artefactos.zip`: estructura y artefactos SDD del proyecto de referencia del profesor. Se tomó como referencia la separación entre constitución, AGENTS, memoria, specs por slice, plantillas, gates, agentes y skills. No se copiaron su stack ni reglas específicas de SQL Server/EF/roles/hosting.
2. Archivo `Descripcion_PetMate_Backend_Movil.pdf`: alcance y requisitos funcionales/técnicos de PetMate.
3. Respuestas del usuario en esta conversación: decisiones explícitas sobre stack, arquitectura, roles, rutas, wrapper, Atlas, Redis, seguridad, costo, prioridad inicial y uso de OpenAPI con Scalar como única UI interactiva de la API.

## No se hizo

- No se creó la solución .NET ni código fuente.
- No se ejecutaron tareas T001–T041.
- No se configuró Azure ni se crearon recursos cloud.
- No se comprobaron precios actuales de Azure, disponibilidad de planes gratuitos ni licencias vigentes en la web. La selección de productos y paquetes queda condicionada a su verificación en el momento de planificar.

## Decisiones que deben ratificarse

La constitución tiene estado `draft-pending-human-ratification`. Las duraciones de tokens, Owner-only initial registration y soft-delete se incluyen como propuestas técnicas iniciales para ser revisadas en los specs antes de implementación.
