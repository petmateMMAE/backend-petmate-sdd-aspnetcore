# Task — pm-034 auth-brute-force-protection

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-030, pm-031, pm-036

## Tasks

### T023 — Pruebas unitarias, integración y seguridad de auth
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana)
- **Spec principal:** pm-034
- **Criterios propios:** ninguno (ver nota)
- **También cubre / nota:** Aporta pruebas de seguridad de autenticación (evidencia para pm-034); su alcance histórico no cambia.
- **Fuente histórica:** [`_historico/01-initial-authentication/task.md`](../_historico/01-initial-authentication/task.md) (sección T023)
- **Evidencia y fases:** ver [plan.md](plan.md) y [checklists/gates.md](checklists/gates.md); la fuente histórica conserva el alcance original.
- **Fases:** [x] Implementación · [x] Pruebas · [x] Revisión independiente (2026-11-08, ver pm-035 gates) · [x] Documentación · [x] Aprobación humana (2026-11-08)

### T056 — Implementar cuota por correo, bloqueo y limitador asíncrono de hashing
- **Tipo:** Nueva
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana)
- **Spec principal:** pm-034
- **Criterios propios:** pm-034#CA-01, pm-034#CA-02, pm-034#CA-03, pm-034#CA-04
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente (no realizada para la Task; la del grupo de autenticación se hace con T025) · [x] Documentación · [x] Aprobación humana (2026-11-08)
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [x] Criterios propios satisfechos con evidencia reproducible.
- [x] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [x] Contratos y documentación actualizados.
- [x] Revisión independiente completada.
- [x] Aprobación humana registrada (2026-11-08; los agentes no se auto-firman).
