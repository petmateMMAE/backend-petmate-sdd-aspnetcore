# Gates — pm-076-load-balancer (T088)

> Estado (2026-11-19): verificado contra Azure en vivo. Sustituye al registro
> estático del 2026-11-18. **CA-02 y CA-03 siguen sin evidencia suficiente.**

## Evidencia recogida

```
$ az containerapp replica list --name petmate-api --resource-group rg-petmate-prod --query "[].name"
petmate-api--0000006-bc8945567-h5rcn
petmate-api--0000006-bc8945567-pnv7n

$ az containerapp show ... --query "properties.configuration.ingress"
external: true, allowInsecure: false
```

Se enviaron 20 peticiones a `/api/v1/health/live`; todas respondieron 200.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-076#CA-01` | Ingress de Container Apps delante de 2 réplicas vivas, confirmadas por nombre | **Cumple** |
| `pm-076#CA-02` | Las 20 peticiones respondieron, pero **no se pudo atribuir cada una a su réplica**: la consulta a Log Analytics falló con `InsufficientAccessError`. Sin esa atribución no hay prueba de reparto | **Sin evidencia** |
| `pm-076#CA-03` | No se retiró ninguna réplica. Hacerlo en producción con testers de Google Play activos no era prudente | **Sin evidencia** |

## Razones para no cerrar

- **CA-02:** la cuenta no tiene permiso de lectura sobre el workspace
  `log-petmate-prod`. Para cerrarlo hace falta asignar el rol *Log Analytics
  Reader*, o que la API devuelva una cabecera con el identificador de réplica.
- **CA-03:** retirar una réplica es una prueba de resiliencia que debe hacerse
  en una ventana acordada, no con la prueba cerrada de Play en curso.

## Cierre

- [x] CA-01 satisfecho con evidencia reproducible.
- [ ] CA-02 — bloqueado por permisos de Log Analytics.
- [ ] CA-03 — pendiente de ventana de prueba.
- [ ] Aprobación humana: **no se firma**, porque 2 de 3 criterios siguen sin evidencia.
