# Gates — pm-071-cloud-redis-cache (T083)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-071#CA-01` | Solo TLS (`--port 0`); sin prueba negativa | Parcial |
| `pm-071#CA-02` | `requirepass` propio por servicio; sin prueba negativa | Parcial |
| `pm-071#CA-03` | Ingress interno; sin prueba desde fuera | Parcial |
| `pm-071#CA-04` | Contraseña distinta (`random_password` por clave del servicio); rotación sin documentar | Parcial |
| `pm-071#CA-05` | Sin prueba ni revisión de logs en la nube | Sin evidencia |
| `pm-071#CA-06` | `--maxmemory-policy allkeys-lru --save "" --appendonly no` | Cumple (revisión estática) |
| `pm-071#CA-07` | Memoria.md: `ready` Healthy en v0.1.0; no repetido | Parcial |
| `pm-071#CA-08` | Degradación segura cubierta por pm-050/pm-051; no probada con el Redis desplegado | Parcial |
| `pm-071#CA-09` | Sin prueba de reinicio | Sin evidencia |
| `pm-071#CA-10` | Cubierto por la separación Auth/Cache en el código (pm-013); no inspeccionado en el Redis desplegado | Parcial |

## Razones para no aprobar aún
- Spec en "Propuesto" y sin aprobación humana.
- Redis autogestionado en lugar de administrado.
- Sin pruebas negativas (CA-01 a CA-03).
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
