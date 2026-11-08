# Task — pm-061 api-docs-contract-sync

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-007, pm-008, pm-009

## Tasks

### T024 — Contratos y documentación de auth
- **Tipo:** Histórica
- **Estado:** En progreso (verificada con evidencia; pendiente de aprobación humana)
- **Spec principal:** pm-061
- **Criterios propios:** pm-061#CA-01
- **También cubre / nota:** Contratos de autenticación (pm-061 CA-01).
- **Fuente histórica:** [`_historico/01-initial-authentication/task.md`](../_historico/01-initial-authentication/task.md) (sección T024)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T039 — Pruebas de contrato y documentación
- **Tipo:** Histórica
- **Estado:** En progreso (verificada con evidencia; pendiente de aprobación humana)
- **Spec principal:** pm-061
- **Criterios propios:** pm-061#CA-02
- **También cubre / nota:** También cubre pm-039, pm-040, pm-041, pm-042, pm-043, pm-044 (pruebas de contrato y documentación de mascotas).
- **Fuente histórica:** [`_historico/03-pet-management/task.md`](../_historico/03-pet-management/task.md) (sección T039)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T073 — Sincronizar contratos, catálogo y OpenAPI
- **Tipo:** Nueva
- **Estado:** En progreso (implementada y verificada; pendiente de aprobación humana)
- **Spec principal:** pm-061
- **Criterios propios:** pm-061#CA-03, pm-061#CA-04
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente · [x] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
