# Task — pm-023 refresh-session-domain

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-022

## Tasks

### T052 — Documentar la evidencia retrospectiva del dominio de sesiones
- **Tipo:** Retrospectiva
- **Estado:** En progreso (retrospectiva documentada; pendiente de revisión y aprobación humana)
- **Spec principal:** pm-023
- **Criterios propios:** pm-023#CA-01, pm-023#CA-02, pm-023#CA-03
- **Origen:** trabajo implementado bajo T013.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `src/PetMate.Domain/Sessions/*`
  - Pruebas: `SessionTests` (A)
  - Filas de gates/task: `_historico/01-initial-authentication/checklists/gates.md` filas 33–39 (H)
  - Revisión independiente existente: Cierre humano de T013 registrado en `task.md` (H)
- **Demostrable con la evidencia existente:** CA-01 a CA-03.
- **Limitaciones:** Ninguna limitación relevante identificada.
- **Fases:** [x] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
