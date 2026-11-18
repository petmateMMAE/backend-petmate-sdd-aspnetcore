# Gates — pm-074-container-registry (T086)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-074#CA-01` | Azure Container Registry | Cumple (revisión estática) |
| `pm-074#CA-02` | El job `deploy` bloquea cada etiqueta tras publicarla y reutiliza la existente. La inmutabilidad es por bloqueo de etiqueta, no a nivel de registro | Parcial |
| `pm-074#CA-03` | Etiqueta = versión + SHA y la versión viaja en `PETMATE_VERSION` y en las etiquetas del recurso | Cumple (revisión estática) |

## Razones para no aprobar aún
- Falta confirmar con `az acr repository show` que las etiquetas publicadas están realmente bloqueadas.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
