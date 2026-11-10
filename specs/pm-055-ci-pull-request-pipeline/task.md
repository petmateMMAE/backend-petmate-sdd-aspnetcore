# Task — pm-055 ci-pull-request-pipeline

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-021

## Tasks

### T012 — CI base DevSecOps
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana) — sin revisión independiente. Implementada el 2026-11-03; el workflow, el escaneo de imagen y el SBOM que le faltaban existen en `pipeline.yml`
- **Spec principal:** pm-055
- **Criterios propios:** ninguno (ver nota)
- **También cubre / nota:** Evidencia local parcial (SAST, formato, NuGet, secret scan, suites) para pm-055 CA-02; el workflow YAML, el escaneo de imagen y el SBOM siguen pendientes. Con el workflow único (`pipeline.yml`), Trivy y Syft ya presentes, su alcance histórico queda cubierto; los criterios de pm-055 siguen siendo de T066 y T067.
- **Fuente histórica:** [`_historico/00-foundation/task.md`](../_historico/00-foundation/task.md) (sección T012)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T066 — Crear el workflow de CI de pull request
- **Tipo:** Nueva
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana) — sin revisión independiente
- **Spec principal:** pm-055
- **Criterios propios:** pm-055#CA-01, pm-055#CA-02
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella) · [x] Documentación · [x] Aprobación humana (2026-11-08)
- **Nota:** la ejecución del workflow actualizado en GitHub llega con el primer push (pull request o `main`).

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
