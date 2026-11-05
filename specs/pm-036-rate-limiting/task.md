# Task — pm-036 rate-limiting

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-017, pm-029, pm-030, pm-031

## Tasks

### T054 — Documentar la evidencia retrospectiva del rate limiting de autenticación
- **Tipo:** Retrospectiva
- **Estado:** Propuesto (retrospectiva por documentar)
- **Spec principal:** pm-036
- **Criterios propios:** pm-036#CA-01, pm-036#CA-02, pm-036#CA-04
- **Origen:** trabajo implementado bajo T018, T019, T020.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `src/PetMate.Api/Configuration/ApplicationBootstrap.cs` (`ConfigureRateLimiting`), `RateLimitKey.cs`
  - Pruebas: `RateLimitAndProxyTests`, `AuthEndpointsTests` (A)
  - Filas de gates/task: `_historico/01-initial-authentication/checklists/gates.md` filas 24–32 (H)
  - Revisión independiente existente: Las revisiones de T018–T020 constan solo en `task.md` (H); no existen como archivos
- **Demostrable con la evidencia existente:** CA-01, CA-02 y CA-04.
- **Limitaciones:** Solo cubre autenticación; la matriz global la cubre T057.
- **Fases (sin completar):** [ ] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

### T057 — Definir la matriz de rate limiting para endpoints no autenticación
- **Tipo:** Nueva
- **Estado:** Propuesto
- **Spec principal:** pm-036
- **Criterios propios:** pm-036#CA-03
- **Verificar:** pruebas y evidencia reproducible por definir en el `plan.md` del Spec.
- **Fases (sin completar):** [ ] Implementación · [ ] Pruebas · [ ] Revisión independiente · [ ] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
