# Gates — pm-083-infrastructure-teardown

> Estado (2026-11-19): revisado y **corregido**. El registro del 2026-11-18
> detectó que CA-03 no se cumplía; este ciclo lo arregla.

## Corrección aplicada

El registro anterior encontró que `azurerm_storage_account.images` formaba parte
del `destroy`. Esa cuenta guarda las fotos de mascotas y negocios que suben los
usuarios: un `terraform destroy` las habría borrado todas, sin forma de
recuperarlas.

Se le añadió protección en `infra/azure/storage.tf`:

```hcl
lifecycle {
  prevent_destroy = true
}
```

Terraform ahora rechaza cualquier destroy que toque esa cuenta. Verificado con
`terraform validate` (configuración válida) y `terraform fmt -check` (sin
cambios de formato).

Para desmantelar el entorno a propósito hay que: respaldar las fotos, quitar la
protección en un commit aparte y recién entonces destruir. Son tres pasos
deliberados, no un comando.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-083#CA-01` | Creación y destrucción por Terraform y el workflow `infra.yml` | **Cumple** |
| `pm-083#CA-02` | Confirmación por texto de quien lanza el flujo. No hay segunda persona: el plan gratuito de GitHub no permite environments con revisores | **Parcial** |
| `pm-083#CA-03` | `prevent_destroy = true` en la cuenta de imágenes; los datos de usuarios ya no se pueden destruir por accidente | **Cumple** |
| `pm-083#CA-04` | El destroy elimina Log Analytics, que es lo esperado al desmantelar | **Cumple** |

## Cierre

- [x] CA-01, CA-03 y CA-04 satisfechos; CA-03 con corrección verificada.
- [ ] CA-02 — limitación del plan gratuito de GitHub, no del diseño.
- [x] Revisión independiente — la brecha la encontró el registro previo y se corrigió aquí.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

> CA-02 queda documentado como limitación de la herramienta. Si el profesor lo
> exige, se puede sustituir por una regla de protección de rama que pida
> revisión de otra persona antes de permitir el workflow de destroy.
