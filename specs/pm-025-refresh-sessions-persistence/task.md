# Task — pm-025 refresh-sessions-persistence

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-012, pm-023

## Tasks

### T053 — Documentar la evidencia retrospectiva de la persistencia de sesiones
- **Tipo:** Retrospectiva
- **Estado:** En progreso (retrospectiva documentada; pendiente de revisión y aprobación humana)
- **Spec principal:** pm-025
- **Criterios propios:** pm-025#CA-01, pm-025#CA-02, pm-025#CA-03, pm-025#CA-04
- **Origen:** trabajo implementado bajo T014.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `src/PetMate.Infrastructure/Persistence/MongoSessionRepository.cs`, `MongoIndexInitializer.cs`
  - Pruebas: `AuthPersistenceIntegrationTests`, `PersistenceSecurityTests` (A)
  - Filas de gates/task: `_historico/01-initial-authentication/checklists/gates.md` filas 33–39 (H)
  - Revisión independiente existente: Cierre humano de T014 registrado en `task.md` (H)
- **Demostrable con la evidencia existente:** CA-01 a CA-04.
- **Limitaciones:** Las pruebas de integración requieren Atlas con base de pruebas.
- **Fases:** [x] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
