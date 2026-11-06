# Task — pm-NNN [slug]

> Cada Task debe ser pequeña, ejecutable, verificable y asociada a criterios de aceptación del Spec (`pm-NNN#CA-NN`). La numeración global continúa desde el último ID; no se reutilizan IDs.

## Dependencias
- Spec: [spec.md](spec.md)
- Plan: `plan.md` (si ya existe)
- Specs previos:

## Tasks

### TNNN — [Verbo + resultado]
- **Tipo:** Histórica | Retrospectiva | Nueva
- **Estado:** Propuesto
- **Spec principal:** pm-NNN (exactamente uno)
- **Criterios propios:** pm-NNN#CA-NN
- **También cubre / nota:** (solo si verifica criterios de otro Spec; indicarlo sin ambigüedad)
- **Crear:**
- **Modificar:**
- **Verificar:** comandos/tests/evidencia reproducible.
- **Fases:** [ ] Implementación · [ ] Pruebas · [ ] Revisión independiente · [ ] Documentación · [ ] Aprobación humana
- **No hacer:** cambios fuera de alcance; recursos Azure reales o que generen cargos sin autorización expresa.
- **Dependencias:**

#### Solo para Tasks retrospectivas
Documentar exclusivamente lo demostrable con evidencia existente, clasificando cada evidencia como automatizada (A), manual (M), revisión estática (S) o documento histórico (H):
- Código existente:
- Pruebas existentes:
- Filas concretas de `gates.md`/`task.md`:
- Revisión independiente existente:
- Demostrable / no demostrado y limitaciones:
Una Task retrospectiva nunca se cierra automáticamente.

## Definition of Done
- [ ] Resultado implementado según el plan.
- [ ] Tests aplicables pasan.
- [ ] Controles de seguridad aplicables pasan.
- [ ] Documentos/contratos actualizados.
- [ ] Gates contienen evidencia.
- [ ] Revisión humana pendiente/completada.
