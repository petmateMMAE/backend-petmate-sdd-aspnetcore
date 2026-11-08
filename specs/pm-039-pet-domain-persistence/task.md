# Task — pm-039 pet-domain-persistence

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-012, pm-033

## Tasks

### T029 — Definir modelo Pet y persistencia
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana)
- **Spec principal:** pm-039
- **Criterios propios:** pm-039#CA-01, pm-039#CA-02, pm-039#CA-03
- **También cubre / nota:** con la implementación se escribieron pruebas unitarias del modelo y de integración contra Atlas (evidencia para CA-04 y CA-05, que siguen siendo de T036 y T037). Campos y borrado físico por decisión del usuario del 2026-11-08.
- **Fases:** [x] Implementación · [x] Pruebas (ver [checklists/gates.md](checklists/gates.md)) · [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella) · [x] Documentación · [x] Aprobación humana (2026-11-08)
- **Fuente histórica:** [`_historico/03-pet-management/task.md`](../_historico/03-pet-management/task.md) (sección T029)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T036 — Pruebas unitarias Pet
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana) — sin revisión independiente
- **Spec principal:** pm-039
- **Criterios propios:** pm-039#CA-04
- **Alcance histórico cubierto:** dominio (`PetTests`, 19), validadores (`PetValidation` en `PetTests` y `Validators_use_the_server_clock_for_the_birth_date`) y handlers (`PetHandlersTests`, 5); reglas y casos límite en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella) · [x] Documentación · [x] Aprobación humana (2026-11-08)
- **Fuente histórica:** [`_historico/03-pet-management/task.md`](../_historico/03-pet-management/task.md) (sección T036)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T037 — Pruebas de integración Atlas
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana) — sin revisión independiente
- **Spec principal:** pm-039
- **Criterios propios:** pm-039#CA-05
- **Alcance histórico cubierto:** CRUD, índice por propietario, consulta de listado servida por ese índice (`explain`), filtros por propietario, borrado concurrente y base de pruebas aislada (`MongoTestGuard` y colecciones con prefijo único). `PetPersistenceIntegrationTests`, 8 pruebas contra Atlas.
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella) · [x] Documentación · [x] Aprobación humana (2026-11-08)
- **Fuente histórica:** [`_historico/03-pet-management/task.md`](../_historico/03-pet-management/task.md) (sección T037)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

## Definition of Done

- [x] Criterios propios satisfechos con evidencia reproducible.
- [x] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [x] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [x] Aprobación humana registrada (2026-11-08; los agentes no se auto-firman).
