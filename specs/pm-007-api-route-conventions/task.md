# Task — pm-007 api-route-conventions

> Las Tasks históricas conservan su identificador, su alcance y su estado literal. Las retrospectivas documentan solo lo demostrable con evidencia existente y nunca se cierran automáticamente.

## Dependencias

- Spec: [spec.md](spec.md)
- Specs previos: pm-005

## Tasks

### T042 — Documentar la evidencia retrospectiva de rutas versionadas
- **Tipo:** Retrospectiva
- **Estado:** Propuesto (retrospectiva por documentar)
- **Spec principal:** pm-007
- **Criterios propios:** pm-007#CA-01, pm-007#CA-02, pm-007#CA-03
- **Origen:** trabajo implementado bajo T007.
- **Evidencia existente** (A = automatizada, M = manual, S = revisión estática, H = documento histórico):
  - Código: `src/PetMate.Api/Program.cs`, controladores versionados y `TechnicalMediatorProbeController`
  - Pruebas: `OpenApiEndpointsTests` (A)
  - Filas de gates/task: `_historico/00-foundation/checklists/gates.md` filas 62–69 (H)
  - Revisión independiente existente: Revisiones de T007 en `_historico/00-foundation/reviews/` (H)
- **Demostrable con la evidencia existente:** CA-01 a CA-03 solo para la ruta técnica y las rutas de autenticación existentes.
- **Limitaciones:** No existe prueba que recorra todas las rutas contra el catálogo (la cubre T055); T007 no demuestra por sí sola estos criterios.
- **Fases (sin completar):** [ ] Documentación de la evidencia revisada · [ ] Revisión independiente · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana; añadir evidencia que no exista.

### T055 — Verificar automáticamente rutas contra el catálogo y documentar reglas de nombrado
- **Tipo:** Nueva
- **Estado:** Propuesto
- **Spec principal:** pm-007
- **Criterios propios:** pm-007#CA-04, pm-007#CA-05
- **Verificar:** pruebas y evidencia reproducible por definir en el `plan.md` del Spec.
- **Fases (sin completar):** [ ] Implementación · [ ] Pruebas · [ ] Revisión independiente · [ ] Documentación · [ ] Aprobación humana
- **No hacer:** cerrar la Task sin aprobación humana.

## Definition of Done

- [ ] Criterios propios satisfechos con evidencia reproducible.
- [ ] Pruebas unitarias, de integración, de seguridad y de contrato aplicables pasan.
- [ ] Contratos y documentación actualizados.
- [ ] Revisión independiente completada.
- [ ] Aprobación humana registrada (los agentes no se auto-firman).
