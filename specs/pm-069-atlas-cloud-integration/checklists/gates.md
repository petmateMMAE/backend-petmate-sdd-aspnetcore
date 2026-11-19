# Gates — pm-069-atlas-cloud-integration (T081)

> Estado (2026-11-19): revisado en el ciclo de cierre. **No cerrado**: los criterios marcados abajo siguen sin evidencia y no se firman. Ver "Lo que falta".

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-069#CA-01` | Lista de acceso restringida a las IPs de salida de la API. No se verificó en Atlas que no exista `0.0.0.0/0` | Parcial |
| `pm-069#CA-02` | El usuario se crea a mano en Atlas fuera de Terraform; su privilegio solo está descrito en la documentación | Parcial |
| `pm-069#CA-03` | Base `petmate_prod` para producción y `petmate_dev`/`petmate_test` definidas en `environments.md`; no se comprobó ausencia de credenciales en logs | Parcial |

## Razones para no aprobar aún
- La lista de acceso se mantiene en un cron de 6 h: ventana en la que una IP nueva queda bloqueada.
- Usuario y base de Atlas no están bajo IaC.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
