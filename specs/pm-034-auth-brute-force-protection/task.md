# Task — pm-034 auth-brute-force-protection

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-030, pm-031, pm-036

## Tasks

### T023 — Pruebas unitarias, integración y seguridad de auth
- **Tipo:** Histórica
- **Estado:** Propuesto
- **Spec principal:** pm-034
- **Criterios propios:** ninguno (ver nota)
- **También cubre / nota:** Aporta pruebas de seguridad de autenticación (evidencia para pm-034); su alcance histórico no cambia.
- **Fuente histórica:** [`_historico/01-initial-authentication/task.md`](../_historico/01-initial-authentication/task.md) (sección T023)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T056 — Implementar cuota por correo, bloqueo y limitador asíncrono de hashing
- **Tipo:** Nueva
- **Estado:** Propuesto
- **Spec principal:** pm-034
- **Criterios propios:** pm-034#CA-01, pm-034#CA-02, pm-034#CA-03, pm-034#CA-04
- **Verificar:** pruebas y evidencia reproducible por definir en el `plan.md` del Spec.
- **Fases (sin completar):** [ ] Implementación · [ ] Pruebas · [ ] Revisión independiente · [ ] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
