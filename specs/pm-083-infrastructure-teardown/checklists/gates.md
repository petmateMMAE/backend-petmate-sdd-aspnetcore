# Gates — pm-083-infrastructure-teardown (T095)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-083#CA-01` | Creación y destrucción por Terraform y workflow | Cumple (revisión estática) |
| `pm-083#CA-02` | Confirmación por texto de quien lanza el flujo; no hay segunda persona | Parcial |
| `pm-083#CA-03` | Atlas se conserva; **las cuentas de almacenamiento de Blob (`storage.tf`) con fotos privadas sí están en el módulo y se destruirían** | No cumple |
| `pm-083#CA-04` | Elimina la observabilidad (Log Analytics está en el módulo) | Parcial |

## Razones para no aprobar aún
- CA-03: `azurerm_storage_account.images` forma parte de `destroy` y contiene datos de usuarios; revisar si debe llevar `prevent_destroy` o salir del módulo.
- No hay registro de un destroy real probado.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
