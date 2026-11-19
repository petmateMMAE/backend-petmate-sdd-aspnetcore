# Gates — pm-078-cloud-observability (T090)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-078#CA-01` | Log Analytics workspace asociado al ambiente. No hay Application Insights ni trazas | Parcial |
| `pm-078#CA-02` | Solo hay una alerta (5xx > 5 en 5 min). Falta una de indisponibilidad (réplicas 0, sondas fallando) | Parcial |
| `pm-078#CA-03` | pm-054 cubre el filtrado de PII en la aplicación; no se revisó lo que llega a Log Analytics | Parcial |

## Razones para no aprobar aún
- No hay alerta de indisponibilidad (CA-02).
- Con `modo_ahorro` la API está en 0 réplicas por diseño: una alerta de disponibilidad daría falsos positivos.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
