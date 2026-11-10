# Plan — pm-060 sdd-integrity-gate

## Enfoque
- `scripts/check-sdd.ps1` se ejecuta en el job `3 · Integridad SDD`, que es bloqueante en la puerta de calidad.
- Se completó la validación para cubrir todos los criterios: CA duplicados o no consecutivos, Tasks con más de un Spec principal y trazabilidad completa (cada criterio con fila y Task propietaria correcta; ninguna fila de un criterio inexistente).
- `scripts/check-sdd.tests.ps1` prueba que la validación falla ante cada inconsistencia (CA-03) y corre en el mismo job.
