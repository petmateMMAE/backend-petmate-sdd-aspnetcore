# Task — pm-017 trusted-proxy-forwarded-headers

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-005

## Tasks

### T047 — Documentar la evidencia retrospectiva de proxies confiables
- **Tipo:** Retrospectiva
- **Estado:** En progreso (retrospectiva documentada; pendiente de revisión y aprobación humana)
- **Spec principal:** pm-017
- **Criterios propios:** pm-017#CA-01, pm-017#CA-02, pm-017#CA-03
- **Origen:** trabajo implementado bajo T018, T019, T020, T021.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `src/PetMate.Api/Program.cs` (`UseForwardedHeaders` condicional), `src/PetMate.Api/Configuration/ApplicationBootstrap.cs` (`ConfigureForwardedHeaders`), `RateLimitKey.cs`
  - Pruebas: `RateLimitAndProxyTests` (A)
  - Filas de gates/task: `_historico/01-initial-authentication/checklists/gates.md` filas 24–32 (H)
  - Revisión independiente existente: Las revisiones de T016–T021 constan solo en `task.md` (H); no existen como archivos
- **Demostrable con la evidencia existente:** CA-01 a CA-03.
- **Limitaciones:** La evidencia es indirecta; la evidencia directa contra spoofing la aporta T097.
- **Fases:** [x] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

### T097 — Verificar el rechazo de cabeceras reenviadas de proxies no confiables
- **Tipo:** Nueva
- **Estado:** En progreso (implementada y verificada; pendiente de aprobación humana)
- **Spec principal:** pm-017
- **Criterios propios:** pm-017#CA-04
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente · [x] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
