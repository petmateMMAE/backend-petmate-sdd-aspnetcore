# Gates — pm-065-cost-guardrails (T077)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-065#CA-01` | `azurerm_consumption_budget_resource_group` con umbrales 50/80/100 y aviso por correo. Definido en código; no hay evidencia de que se aplicó ni de que llegó un aviso | Parcial |
| `pm-065#CA-02` | `tags = local.tags` en VNet, ACR, Log Analytics, Container Apps, alertas y almacenamiento. No se auditó con `az resource list` | Parcial |
| `pm-065#CA-03` | El procedimiento ante agotamiento de créditos solo está en una frase de `azure-deployment.md` ("la suscripción se deshabilita, no genera cargos"); no describe qué hacer ni cómo evitar quedar con una sola instancia | No cumple |

## Razones para no aprobar aún
- CA-03 sin procedimiento propio.
- El presupuesto está en USD; pm-084 CA-03 exige demostrar el costo en MXN.
- Sin evidencia de que el presupuesto exista en la suscripción.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
