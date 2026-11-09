# Task — pm-036 rate-limiting

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-017, pm-029, pm-030, pm-031

## Tasks

### T054 — Documentar la evidencia retrospectiva del rate limiting de autenticación
- **Tipo:** Retrospectiva
- **Estado:** Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva)
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
- **Fases:** [x] Documentación de la evidencia revisada · [ ] Revisión independiente (no realizada para la Task; la del grupo de autenticación se hace con T025) · [x] Aprobación humana (2026-11-08)
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

### T057 — Definir la matriz de rate limiting para endpoints no autenticación
- **Tipo:** Nueva
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana)
- **Spec principal:** pm-036
- **Criterios propios:** pm-036#CA-03
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente (no realizada para la Task; la del grupo de autenticación se hace con T025) · [x] Documentación · [x] Aprobación humana (2026-11-08)
- **No hacer:** cerrar la Task sin aprobación humana.

### T100 — Límite por IP distribuido en Redis Auth, validación de proxies al arrancar y documentación de saltos
- **Tipo:** Nueva
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad)
- **Spec principal:** pm-036
- **Criterios propios:** pm-036#CA-05, pm-036#CA-06
- **Origen:** hallazgo de seguridad H6; Aprobado por el usuario el 2026-11-08 (opciones A de H7, H3 y H8; B de H4; A, B y C de H6).
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente · [x] Documentación · [x] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [x] Criterios propios satisfechos con evidencia reproducible.
- [x] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [x] Contratos y documentación actualizados.
- [x] Revisión independiente completada.
- [x] Aprobación humana registrada (2026-11-08; los agentes no se auto-firman).
