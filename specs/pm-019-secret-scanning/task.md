# Task — pm-019 secret-scanning

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-004

## Tasks

### T049 — Documentar la evidencia retrospectiva del escaneo de secretos
- **Tipo:** Retrospectiva
- **Estado:** Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva)
- **Spec principal:** pm-019
- **Criterios propios:** pm-019#CA-01, pm-019#CA-02
- **Origen:** trabajo implementado bajo T012.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `scripts/quality-gate.ps1`
  - Pruebas: `SecretScanTests` (A)
  - Filas de gates/task: `_historico/00-foundation/checklists/gates.md` filas 86–87 y 89 (H)
  - Revisión independiente existente: Ninguna revisión independiente existente
- **Demostrable con la evidencia existente:** CA-01 y CA-02.
- **Limitaciones:** T012 sigue abierta; la evidencia es local.
- **Fases:** [x] Documentación de la evidencia revisada · [ ] Revisión independiente · [x] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
