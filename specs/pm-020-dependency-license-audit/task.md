# Task — pm-020 dependency-license-audit

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-003

## Tasks

### T050 — Documentar la evidencia retrospectiva de la auditoría de dependencias y licencias
- **Tipo:** Retrospectiva
- **Estado:** Propuesto (retrospectiva por documentar)
- **Spec principal:** pm-020
- **Criterios propios:** pm-020#CA-01, pm-020#CA-02, pm-020#CA-03
- **Origen:** trabajo implementado bajo T012.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `scripts/quality-gate.ps1`, `scripts/quality-exceptions.json`
  - Pruebas: Ejecución del gate local (M)
  - Filas de gates/task: `_historico/00-foundation/checklists/gates.md` filas 86–87 y 89 (H)
  - Revisión independiente existente: Ninguna revisión independiente existente
- **Demostrable con la evidencia existente:** CA-01 a CA-03.
- **Limitaciones:** Excepción xunit Legacy vigente hasta 2027-02-03; evidencia local.
- **Fases (sin completar):** [ ] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
