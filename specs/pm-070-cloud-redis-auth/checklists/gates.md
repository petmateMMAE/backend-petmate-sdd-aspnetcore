# Gates — pm-070-cloud-redis-auth (T082)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-070#CA-01` | `--port 0 --tls-port 6379`: solo TLS. No se probó una conexión sin TLS | Parcial |
| `pm-070#CA-02` | `requirepass` con `random_password`. No se probó una conexión sin credenciales | Parcial |
| `pm-070#CA-03` | `external_enabled = false` (solo dentro del ambiente de Container Apps). No se probó desde fuera | Parcial |
| `pm-070#CA-04` | Contraseña por secreto del entorno; rotación sin documentar | Parcial |
| `pm-070#CA-05` | Sin prueba ni revisión de logs para este caso en la nube | Sin evidencia |
| `pm-070#CA-06` | `noeviction` y AOF `everysec` (`apps.tf`) | Cumple (revisión estática) |
| `pm-070#CA-07` | Memoria.md: v0.1.0 con `ready` Healthy con Mongo y los dos Redis (2026-11-06, no repetido en este registro) | Parcial |
| `pm-070#CA-08` | Cubierto en el código por pm-028/T017 y T098; no probado contra el Redis desplegado | Parcial |
| `pm-070#CA-09` | Hay reparación de AOF antes de arrancar (commit 7650f45), prueba de reinicio no registrada | Parcial |

## Razones para no aprobar aún
- El spec está en "Propuesto" y sin aprobación humana: no puede cerrarse.
- Es un contenedor Redis autogestionado (sin parches ni HA administrados); el spec dice "administrado".
- Con `modo_ahorro` Redis Auth tiene horario y puede estar apagado: afecta CA-08 y CA-09.
- `--tls-auth-clients no`: TLS sin certificado de cliente (válido, pero debe quedar decidido).
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
