# Gates — pm-066-iac-baseline (T078)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-066#CA-01` | ADR-004 existe pero su estado es "Propuesto" (sin aprobación humana) | Parcial |
| `pm-066#CA-02` | `infra.yml` separa `plan`/`apply`/`destroy`, exige `main` y confirmación textual para destroy. El plan gratuito de GitHub no permite environments con revisores, así que la "autorización" es solo el permiso de escritura (decisión registrada en el workflow) | Parcial |
| `pm-066#CA-03` | `variables.tf` sin valores secretos; llegan como `TF_VAR_*` desde secretos de GitHub. No se ejecutó un escáner de secretos específico sobre `infra/` | Parcial |

## Razones para no aprobar aún
- ADR-004 sin aprobar.
- La autorización de `apply` no es una aprobación de persona distinta de quien lo lanza.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
