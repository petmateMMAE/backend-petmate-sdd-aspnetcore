# Gates — pm-068-cloud-secrets-configuration (T080)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-068#CA-01` | Secretos de Container Apps en `apps.tf`; sin valores en archivos versionados | Parcial |
| `pm-068#CA-02` | `.env` no entra a la imagen (`.dockerignore`) y la producción recibe variables del entorno; no se auditó la imagen publicada | Parcial |
| `pm-068#CA-03` | No existe documento de rotación de JWT, Mongo, Redis ni correo | No cumple |
| `pm-068#CA-04` | Key Vault no se usa, por lo que la condición se cumple por omisión | Parcial |

## Razones para no aprobar aún
- CA-03: falta el procedimiento de rotación documentado.
- Cambiar un secreto exige un `apply` completo.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
