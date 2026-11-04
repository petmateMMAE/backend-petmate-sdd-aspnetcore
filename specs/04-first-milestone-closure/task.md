# Task — 04 Cierre del primer hito

> Tarea global T041. Solo se ejecuta después de T001–T040.

## T041 — Validación end-to-end y aceptación del primer hito
- **Estado:** Propuesto
- **Ejecutar:** flujo integral Register → Login → GET/PUT perfil → Create/GetAll/GetById/Update/Delete pet → Refresh → Logout.
- **Crear/ajustar:** pruebas end-to-end y reporte de aceptación sin duplicar las pruebas unitarias de cada slice.
- **Verificar:** 11 endpoints, wrappers, códigos HTTP, refresh rotation, replay detection, blacklist Redis, Mongo Atlas test DB, separación Owner A/Owner B e indisponibilidad segura de Redis Auth.
- **Ejecutar:** Unit, Integration, Security y Contract en Release; SAST, secret scan, dependency/license audit y container scan cuando aplique.
- **Revisar:** `@security-reviewer` y `@traceability-reviewer`; resolver todo hallazgo bloqueante.
- **Actualizar:** `docs/api/endpoints.md`, `Memoria.md`, `specs/README.md` y gates con estado real.
- **No hacer:** desplegar a recursos Azure con costo, implementar módulos posteriores o autoaprobar el hito.
- **Cierre:** el agente informa evidencia y solicita aprobación explícita del usuario. Solo entonces marcar el primer hito como Cerrado.
