# Task — pm-055 ci-pull-request-pipeline

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-021

## Tasks

### T012 — CI base DevSecOps
- **Tipo:** Histórica
- **Estado:** Implementada; NO cerrada (2026-11-03) (implementada pero abierta; sin cambio)
- **Spec principal:** pm-055
- **Criterios propios:** ninguno (ver nota)
- **También cubre / nota:** Evidencia local parcial (SAST, formato, NuGet, secret scan, suites) para pm-055 CA-02; el workflow YAML, el escaneo de imagen y el SBOM siguen pendientes. Implementada, no cerrada.
- **Fuente histórica:** [`_historico/00-foundation/task.md`](../_historico/00-foundation/task.md) (sección T012)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T066 — Crear el workflow de CI de pull request
- **Tipo:** Nueva
- **Estado:** En progreso (verificada; pendiente de revisión independiente y aprobación humana)
- **Spec principal:** pm-055
- **Criterios propios:** pm-055#CA-01, pm-055#CA-02
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente · [x] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

### T067 — Configurar ramas protegidas y checks requeridos
- **Tipo:** Nueva
- **Estado:** Propuesto
- **Spec principal:** pm-055
- **Criterios propios:** pm-055#CA-03, pm-055#CA-04
- **Verificar:** pruebas y evidencia reproducible por definir en el `plan.md` del Spec.
- **Fases (sin completar):** [ ] Implementación · [ ] Pruebas · [ ] Revisión independiente · [ ] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
