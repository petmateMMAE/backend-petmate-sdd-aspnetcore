# Gates — pm-075-backend-two-instances (T087)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-075#CA-01` | `modo_ahorro = false` fija 2 réplicas. Con el valor por defecto (`true`) son 0 a 2. El despliegue v0.1.0 registró 2 réplicas | Parcial |
| `pm-075#CA-02` | API stateless por diseño; sin prueba de reparto entre réplicas registrada | Parcial |
| `pm-075#CA-03` | Sondas en `apps.tf` (`transport = "HTTP"`) hacia `/api/v1/health/live` y `ready` | Cumple (revisión estática) |
| `pm-075#CA-04` | Sin volúmenes en la API; las fotos van a Blob | Cumple (revisión estática) |

## Razones para no aprobar aún
- El valor por defecto (`modo_ahorro = true`) no cumple CA-01: el spec solo se cumple al aplicar con `MODO_AHORRO=false`.
- No hay evidencia vigente (solo la de 2026-11-06).
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
