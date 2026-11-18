# Gates — pm-067-iac-state-management (T079)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-067#CA-01` | Backend remoto en Blob (cifrado en reposo por defecto de Azure). El bloqueo del estado lo da el arrendamiento (lease) del blob; no se probó | Parcial |
| `pm-067#CA-02` | Sin claves compartidas ni acceso público. El estado puede contener secretos (contraseñas de Redis, URI de Mongo en el recurso de la app): el control es el acceso por identidad, no su ausencia | Parcial |
| `pm-067#CA-03` | Costo de la cuenta de estado estimado en < 1 USD/mes en el estudio; "sin cargos" no se demuestra porque se paga con crédito | Parcial |

## Razones para no aprobar aún
- El estado contiene secretos por diseño de Terraform; no hay evidencia de revisión de quién puede leerlo.
- No se probó el bloqueo.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
