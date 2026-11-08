# Task — pm-021 local-quality-gate

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-018, pm-019, pm-020

## Tasks

### T051 — Documentar la evidencia retrospectiva del gate de calidad local
- **Tipo:** Retrospectiva
- **Estado:** En progreso (retrospectiva documentada; pendiente de revisión y aprobación humana)
- **Spec principal:** pm-021
- **Criterios propios:** pm-021#CA-01, pm-021#CA-02
- **Origen:** trabajo implementado bajo T012.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `scripts/quality-gate.ps1`
  - Pruebas: Ejecución local del gate (M)
  - Filas de gates/task: `_historico/00-foundation/checklists/gates.md` filas 86–87 y 89 (H)
  - Revisión independiente existente: Ninguna revisión independiente existente
- **Demostrable con la evidencia existente:** CA-01 y CA-02.
- **Limitaciones:** El gate aún no corre en CI.
- **Fases:** [x] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
