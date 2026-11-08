# Gates — pm-059 release-versioning (T071)

> Estado: T071 verificada el 2026-11-08; pendiente de confirmar la interpretación de "autorización expresa" (CA-02), revisión independiente y aprobación humana.

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-059#CA-01` | `VERSION` = `0.2.0`; el paso "Versión" de `deploy` falla si el valor no es SemVer. | Cumple |
| `pm-059#CA-02` | La etiqueta `api-vX.Y.Z` solo la crea `deploy` tras despliegue y humo correctos en `main`; los pull request no etiquetan. | Cumple si se acepta que el merge/push a `main` o `workflow_dispatch` es la autorización expresa |
| `pm-059#CA-03` | Imagen `X.Y.Z-<sha7>` bloqueada en ACR, etiquetas OCI de versión y revisión, `PETMATE_VERSION` y sufijo de revisión. | Cumple |

## Evidencia (2026-11-08)
- Revisión estática de `pipeline.yml` y `docs/versioning/version-matrix.md`.
- Corrección de documentación: la matriz decía que la etiqueta Git dispara el CD; en realidad la crea el CD al terminar bien.
- Observación: el paso de etiqueta trata cualquier fallo de `git push` como "ya existía" (no bloqueante).
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T071.
