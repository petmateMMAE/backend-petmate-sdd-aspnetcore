# Task — pm-031 refresh-token-rotation

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-025, pm-027, pm-030

## Tasks

### T020 — Refresh Token persistente y rotatorio
- **Tipo:** Histórica
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana)
- **Spec principal:** pm-031
- **Criterios propios:** pm-031#CA-01, pm-031#CA-02, pm-031#CA-03
- **Fuente histórica:** [`_historico/01-initial-authentication/task.md`](../_historico/01-initial-authentication/task.md) (sección T020)
- **Evidencia y fases:** conservadas en la fuente histórica y en su `checklists/gates.md`; no se reescriben.

### T099 — Ventana de gracia del refresh ante solicitudes concurrentes o reintentos
- **Tipo:** Nueva
- **Estado:** Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad)
- **Spec principal:** pm-031
- **Criterios propios:** pm-031#CA-04
- **Origen:** hallazgo de seguridad H8; Aprobado por el usuario el 2026-11-08 (opciones A de H7, H3 y H8; B de H4; A, B y C de H6).
- **Verificar:** ver [plan.md](plan.md) y la evidencia en [checklists/gates.md](checklists/gates.md).
- **Fases:** [x] Implementación · [x] Pruebas · [ ] Revisión independiente · [x] Documentación · [x] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
