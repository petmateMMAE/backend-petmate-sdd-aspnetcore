# Gates — pm-059 release-versioning (T071)

> Estado: T071 verificada el 2026-11-08; interpretación de "autorización expresa" (CA-02) resuelta por el usuario el 2026-11-08 (opción 2: etiqueta solo con `workflow_dispatch` y la casilla `create_tag`); pendiente de revisión independiente y aprobación humana.

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-059#CA-01` | `VERSION` = `0.2.0`; el paso "Versión" de `deploy` falla si el valor no es SemVer. | Cumple |
| `pm-059#CA-02` | La etiqueta `api-vX.Y.Z` solo la crea `deploy` tras despliegue y humo correctos en `main`; los pull request no etiquetan. | Cumple: el merge a `main` no etiqueta; solo `workflow_dispatch` con `create_tag` (verificado por `ReleaseTagAuthorizationTests`; la ejecución real en GitHub aún no se ha hecho) |
| `pm-059#CA-03` | Imagen `X.Y.Z-<sha7>` bloqueada en ACR, etiquetas OCI de versión y revisión, `PETMATE_VERSION` y sufijo de revisión. | Cumple |

## Evidencia (2026-11-08)
- Revisión estática de `pipeline.yml` y `docs/versioning/version-matrix.md`.
- Corrección de documentación: la matriz decía que la etiqueta Git dispara el CD; en realidad la crea el CD al terminar bien.
- Observación: el paso de etiqueta trata cualquier fallo de `git push` como "ya existía" (no bloqueante).
- [x] Revisión independiente (2026-11-08): subagente de solo lectura, apto con observaciones; corregidas las de documentación, pruebas, entrada sin sanear, errores ocultados y versión/etiqueta (ver Memoria.md).
- [x] Aprobación humana de T071 (2026-11-08) del cierre de T071.

## Revisión independiente y correcciones (2026-11-08)
- Veredicto: apto con observaciones. Corregido: `version-matrix.md`, `pipeline.md` y la guía de despliegue ya no dicen que el merge etiqueta; la entrada `version` se pasa por `env` (sin interpolar en el script); el paso de etiqueta solo tolera «ya existe» (`git ls-remote`) y falla ante cualquier otro error; con `create_tag` la versión debe coincidir con `VERSION` (se comprueba antes de desplegar); `ReleaseTagAuthorizationTests` ahora tiene 5 pruebas (condición, entrada booleana, una sola creación y sin `--tags`/`gh release`, job `deploy` solo en `main`, entrada por entorno).
- Riesgo documentado, no resuelto: `terraform apply` puede devolver `PETMATE_VERSION` a `var.api_version` (solo la imagen está en `ignore_changes`).
- Limitación: nada se ha ejecutado en GitHub; el permiso `contents: write` del job `deploy` sigue activo en cada merge aunque ya no etiquete.
- Pruebas: Security 185/185 (medido el 2026-11-08 tras estos cambios).
- Aprobación humana de T071 registrada el 2026-11-08 (el usuario aprobó y cerró la Task).
