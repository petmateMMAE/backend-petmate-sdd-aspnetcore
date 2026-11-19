# Gates — pm-072-cloud-network-security (T084)

> Estado (2026-11-19): revisado en el ciclo de cierre. **No cerrado**: los criterios marcados abajo siguen sin evidencia y no se firman. Ver "Lo que falta".

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-072#CA-01` | Segmentación por subred y `default_action = "Deny"` en la cuenta de Redis. Hay una sola subred: no hay segmentación entre API y Redis | Parcial |
| `pm-072#CA-02` | No hay NSG ni reglas de salida declaradas en Terraform; la salida está abierta | No cumple |
| `pm-072#CA-03` | Sin pruebas de acceso denegado registradas | Sin evidencia |

## Razones para no aprobar aún
- CA-02 y CA-03 sin evidencia.
- Una subred única y salida abierta no son "reglas mínimas".
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
