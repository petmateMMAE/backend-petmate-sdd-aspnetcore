# Gates — pm-077-cd-deployment-pipeline (T089)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-077#CA-01` | Etiqueta `VERSION-sha`, imagen bloqueada en ACR y etiqueta Git `api-vX` solo con `create_tag` manual | Cumple (revisión estática) |
| `pm-077#CA-02` | Prueba de humo por HTTPS en el job `smoke-test`/`deploy` | Cumple (revisión estática) |
| `pm-077#CA-03` | Reversa automática a la revisión previa | Cumple (revisión estática) |
| `pm-077#CA-04` | El despliegue sale con cada push a `main` sin aprobación de persona: el plan gratuito de GitHub no permite environments con revisores. El control es la protección de ramas (T067) y el PR | Parcial |

## Razones para no aprobar aún
- CA-04: sin aprobación humana específica de release ni revisión de costo previa al despliegue.
- Falta una ejecución verde reciente como evidencia (la última registrada es de `62dda53`).
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
