# Gates — pm-076-load-balancer (T088)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-076#CA-01` | Servicio equivalente de Azure delante de las instancias (ingress de Container Apps) | Cumple (revisión estática) |
| `pm-076#CA-02` | No hay evidencia de reparto (ni cabecera ni log que identifique la réplica) | Sin evidencia |
| `pm-076#CA-03` | Sin prueba de retirar una réplica. El pipeline lista réplicas, pero no la retira | Sin evidencia |

## Razones para no aprobar aún
- CA-02 y CA-03 sin evidencia.
- Se debe aceptar formalmente que el ingress cuenta como "Load Balancer o equivalente".
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
