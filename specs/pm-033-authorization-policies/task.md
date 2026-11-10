# Task — pm-033 authorization-policies

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-027, pm-028

## Tasks

### T022 — Políticas de autenticación y autorización
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana) — sin revisión independiente
- **Spec principal:** pm-033
- **Criterios propios:** pm-033#CA-01, pm-033#CA-02, pm-033#CA-03, pm-033#CA-04
- **Fuente histórica:** [`_historico/01-initial-authentication/task.md`](../_historico/01-initial-authentication/task.md) (sección T022)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.
- **Desglose de ejecución propuesto (pasos internos de T022; no son Tasks nuevas, no consumen IDs y no cambian el estado):** ver [plan.md](plan.md) §10. 1) pruebas primero; 2) crear `AuthorizationPolicies` y registrar respaldo, política por defecto y políticas por rol; 3) `[AllowAnonymous]` explícito en Health, probe técnico, OpenAPI y Scalar; 4) ejecutar pruebas y regresión; 5) documentar en `docs/api/endpoints.md` y en el checklist ASVS; 6) ejecutar el gate de calidad y registrar evidencia en [checklists/gates.md](checklists/gates.md); 7) revisiones independientes y aprobación humana.
- **Alcance y nota sobre la fuente histórica:** el alcance histórico de T022 no se modifica. Su frase «aplicar la política Owner a los endpoints iniciales de mascotas» no puede cumplirse todavía porque esos endpoints no existen; por decisión aprobada P-01 (2026-11-05) las políticas se demuestran con controladores de prueba y `RequireOwner` se aplicará en pm-040 a pm-044. Actualización (2026-11-08): pm-040 a pm-044 ya existen y `PetsController` lleva `[Authorize(Policy = RequireOwner)]` (401 sin token, 403 con rol Business, probado en `PetEndpointsTests`), así que ese alcance histórico quedó cumplido; la trazabilidad de seguridad del módulo sigue en pm-045 CA-02.
- **Fases:** [x] Implementación de las políticas · [x] Pruebas (evidencia en [checklists/gates.md](checklists/gates.md)) · [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella) · [x] Documentación del plan y de la evidencia · [x] Aplicación de Owner a los endpoints de mascotas (pm-040 a pm-044, trazabilidad en pm-045 CA-02) · [x] Aprobación humana (2026-11-08)

## Definition of Done

- [x] Criterios propios satisfechos con evidencia reproducible.
- [x] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [x] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [x] Aprobación humana registrada (2026-11-08; los agentes no se auto-firman).
