# Gates — pm-057 container-image-scan (T069)

> Estado: T069 verificada el 2026-11-08; queda abierta la decisión sobre `ignore-unfixed` (ver `plan.md`).

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-057#CA-01` | Trivy `aquasecurity/trivy-action@v0.35.0` escanea la imagen en `docker-build` (PR y main) y en `deploy` antes de publicarla. Herramienta $0. | Cumple |
| `pm-057#CA-02` | `severity: CRITICAL,HIGH` y `exit-code: '1'` bloquean el job; `deploy` depende de `docker-build`. | Cumple para hallazgos con parche. Con `ignore-unfixed: true` los HIGH/CRITICAL sin parche no bloquean: decisión pendiente del usuario |

## Evidencia (2026-11-08)
- Revisión estática de `pipeline.yml` (job `docker-build` y paso "Escaneo de la imagen (Trivy)" de `deploy`).
- No se ejecutó Trivy localmente. El cambio de `pipeline.yml` todavía no se ejecutó en GitHub: no se ha hecho push (instrucción del usuario). La evidencia en CI llegará con la siguiente ejecución del pipeline.
- [ ] Decisión sobre `ignore-unfixed`.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T069.
