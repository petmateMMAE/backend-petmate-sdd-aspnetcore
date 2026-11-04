# PetMate Backend — SDD

Este repositorio utiliza Spec-Driven Development para definir, planificar, implementar y verificar el backend de PetMate mediante agentes de IA.

## Cómo comenzar

1. Lee `AGENTS.md`.
2. Revisa `.specify/memory/constitution.md` y `Memoria.md`.
3. Consulta el índice `specs/README.md`.
4. Revisa los cuatro artefactos del slice que corresponda.
5. No ejecutes tareas de implementación hasta que el usuario haya aprobado el spec, plan, tareas y gates.

## Fuentes de verdad

- Reglas no negociables: `.specify/memory/constitution.md`.
- Operación de agentes: `AGENTS.md`.
- Estado y decisiones: `Memoria.md`.
- Roadmap SDD: `specs/README.md`.
- Contrato de endpoints: `docs/api/endpoints.md`.
- Alcance derivado del PDF PetMate: `docs/product-scope.md`.

Los README y documentos auxiliares no sustituyen la constitución ni las especificaciones.

## Documentación de API

La API genera un documento OpenAPI y utiliza Scalar como única UI interactiva. Swagger UI y otras UI alternativas no forman parte del stack. La configuración inicial se realiza en T007 de `00-foundation`.

## Primer hito

El primer hito de implementación es T001–T041: base de solución, autenticación con renovación y revocación de sesiones, perfil Owner y siete endpoints funcionales de perfil/mascotas. La frontera exacta está descrita en `specs/README.md`.
