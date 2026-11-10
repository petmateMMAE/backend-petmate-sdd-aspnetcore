# Gates — pm-058 sbom-generation (T070)

> Estado: T070 verificada el 2026-11-08; pendiente de revisión independiente y aprobación humana.

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-058#CA-01` | `anchore/sbom-action@v0` (Syft, $0) genera SPDX-JSON de la imagen en cada ejecución del job `docker-build`. | Cumple |
| `pm-058#CA-02` | `artifact-name: sbom-petmate-api-<sha>.spdx.json`: el SBOM queda como artefacto de la ejecución. | Cumple |

## Evidencia (2026-11-08)
- Revisión estática de `pipeline.yml`. El cambio de `pipeline.yml` todavía no se ejecutó en GitHub: no se ha hecho push (instrucción del usuario). La evidencia en CI llegará con la siguiente ejecución del pipeline.
- Observación: la acción usa la etiqueta móvil `@v0`; fijarla a una versión concreta reduciría el riesgo de cadena de suministro (no bloqueante).
- [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella).
- [x] Aprobación humana del cierre de T070 (2026-11-08).
