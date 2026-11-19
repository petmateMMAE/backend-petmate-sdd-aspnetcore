# Gates — pm-066-iac-baseline

> Estado (2026-11-19): revisado. El bloqueo del registro del 2026-11-18 era el
> ADR-004 sin aprobar; ya está aprobado.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-066#CA-01` | **ADR-004 aprobado** por Adrián Adonai García Jiménez el 2026-11-19. Elige Terraform con los proveedores `azurerm`, `tls`, `random` y `mongodbatlas` | **Cumple** |
| `pm-066#CA-02` | `infra.yml` separa `plan`, `apply` y `destroy`; exige rama `main` y confirmación textual para destruir | **Cumple con limitación** |
| `pm-066#CA-03` | `variables.tf` no contiene valores secretos: llegan como `TF_VAR_*` desde secretos de GitHub. El pipeline corre gitleaks sobre todo el repositorio, `infra/` incluido | **Cumple** |

## Limitación registrada (CA-02)

La "autorización" para `apply` es el permiso de escritura al repositorio, no la
aprobación de una persona distinta de quien lo lanza. El plan gratuito de GitHub
no permite *environments* con revisores obligatorios.

No es un defecto del diseño sino del plan contratado. Si el profesor lo exige,
se sustituye por una regla de protección de rama que pida revisión de otra
persona.

## Cierre

- [x] CA-01 y CA-03 satisfechos; CA-02 cumplido salvo la limitación del plan.
- [x] Revisión independiente — ADR revisado y aprobado explícitamente.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.
