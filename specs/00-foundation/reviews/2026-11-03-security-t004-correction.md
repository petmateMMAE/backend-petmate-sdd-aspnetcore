# Revisión independiente de seguridad — corrección T004

- **Fecha:** 2026-11-03
- **Revisor:** `security-reviewer` (solo lectura)
- **Alcance:** corrección de H-05/H-06 en `Program`, bootstrap, lector `.env`, plantilla y pruebas T004.
- **Veredicto:** **PASS para H-05/H-06.** No hay hallazgos críticos o altos que bloqueen CA-005.

## Resultado

- `ApplicationBootstrap` crea primero el host y consulta el entorno efectivo mediante `builder.Environment.IsProduction()`; en Production, incluido su valor por defecto, no invoca el lector `.env`.
- Fuera de Production, el bootstrap solo consulta `<ContentRootPath>/.env`; no recorre directorios padre.
- Las pruebas de bootstrap ejercitan la misma ruta usada por `Program`: carga en Development y omisión con Production efectivo por defecto.
- Una variable de entorno presente, incluso vacía, conserva prioridad y está documentada/probada.
- No se detectaron secretos reales, logs de valores ni paquetes NuGet nuevos.

## Observación no bloqueante

Las APIs públicas heredadas `Load` y `LoadIntoEnvironment` aún pueden buscar en directorios padre, pero el flujo de arranque no las usa. Conviene restringirlas o retirarlas en una tarea autorizada futura.

No se modificó código ni documentación durante la revisión.
