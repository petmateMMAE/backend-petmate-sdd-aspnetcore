# Checklist de PR — PetMate Backend

> Plantilla. Duplicar o completar por PR; no marcar checks sin evidencia.

## SDD y trazabilidad
- [ ] Spec aprobado y enlazado.
- [ ] Plan y tareas corresponden al cambio real.
- [ ] Gates del slice actualizados con evidencia.
- [ ] No se modificó la constitución sin aprobación.

## Calidad y pruebas
- [ ] Build Release correcto.
- [ ] Pruebas unitarias correctas.
- [ ] Pruebas de integración correctas usando Atlas de pruebas y Redis no productivos.
- [ ] Pruebas de seguridad correctas.
- [ ] Pruebas de contrato correctas si se modificó API/Response Wrapper.

## DevSecOps
- [ ] SAST sin hallazgos bloqueantes.
- [ ] Escaneo de secretos sin hallazgos.
- [ ] Auditoría NuGet de vulnerabilidades y licencias sin hallazgos bloqueantes.
- [ ] Escaneo de contenedor/configuración cuando aplique.
- [ ] Requisitos aplicables de OWASP ASVS L2 revisados.

## Contrato y operación
- [ ] `docs/api/endpoints.md` actualizado.
- [ ] Response Wrapper y códigos HTTP correctos.
- [ ] Sin secretos ni PII en logs/docs/fixtures.
- [ ] Sin dependencias ni recursos con costo distinto de $0.
- [ ] Memoria actualizada sin duplicar la spec.
- [ ] Revisión independiente de seguridad y trazabilidad.
- [ ] Aprobación humana registrada.
