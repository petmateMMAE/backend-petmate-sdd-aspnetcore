# Qué hacer si se agotan los créditos

> Spec: `pm-065` (CA-03) · Aprobado por Adrián Adonai García Jiménez, 2026-11-19

El proyecto corre con **Azure for Students**: crédito limitado, sin tarjeta
asociada. Si el crédito se agota, Azure **deshabilita la suscripción** en vez de
cobrar. La API deja de responder por completo: no se degrada, se apaga.

Este documento dice cómo evitar llegar ahí y qué hacer si se llega.

## La regla que no se rompe

**Nunca se baja a una sola instancia para ahorrar.** La constitución (§8.3) y
`pm-075` exigen dos instancias con balanceador: es requisito académico, no una
preferencia. Bajar a una incumple el proyecto para ahorrar unos dólares.

Si hay que recortar, se recorta de la lista de abajo, en ese orden, y las dos
instancias se quedan hasta el final.

## Vigilancia

| Umbral | Qué llega | Qué hacer |
|---|---|---|
| 50 % | Correo del presupuesto | Nada. Es informativo |
| 80 % | Correo del presupuesto | Revisar el gasto por recurso y aplicar los recortes 1 y 2 |
| 100 % | Correo del presupuesto | Aplicar todos los recortes y avisar al equipo el mismo día |

El presupuesto es `budget-petmate-prod`, mensual, 40 USD. Para ver el gasto:

```powershell
az consumption usage list --start-date (Get-Date -Format "yyyy-MM-01") `
  --end-date (Get-Date -Format "yyyy-MM-dd") `
  --query "[].{recurso:instanceName,costo:pretaxCost}" -o table
```

## Recortes, en orden

**1. Volver al modo ahorro fuera de horario de revisión.**

```powershell
gh variable set MODO_AHORRO --body "true" --repo petmateMMAE/backend-petmate-sdd-aspnetcore
# y aplicar: terraform apply
```

Baja de ~27 a ~14 USD/mes. La API escala a cero sin tráfico y la primera
petición tarda 10–20 s en despertarla.

**Cuidado:** con `modo_ahorro = true` el mínimo de réplicas vuelve a 0 y
`pm-075#CA-01` deja de cumplirse. Solo es aceptable **fuera** de los días de
evaluación y de la prueba cerrada de Google Play. Hay que volver a `false` antes
de cualquier revisión.

**2. Apagar Redis de caché.** Es el de menor impacto: la API lo usa para
acelerar lecturas y sabe degradarse sin él. El de autenticación **no se toca**:
falla cerrado y sin él la API responde 503.

**3. Reducir la retención de Log Analytics** de 30 a 7 días. Los registros
viejos se pierden, pero la observabilidad en vivo sigue.

**4. Borrar imágenes viejas del registro.** Solo las que no estén desplegadas ni
sean la revisión anterior, que hace falta para la reversa.

## Si la suscripción ya se deshabilitó

1. **No crear recursos nuevos.** La infraestructura sigue definida en Terraform:
   se reconstruye cuando haya crédito.
2. **Verificar que Atlas sigue vivo.** Mongo está fuera de Azure, en su propia
   capa gratuita: los datos no se pierden cuando Azure se apaga.
3. **Comprobar que la cuenta de imágenes no se borró.** Tiene
   `prevent_destroy = true`, pero una suscripción deshabilitada el tiempo
   suficiente termina liberando recursos. Si hay aviso de eliminación, respaldar
   los contenedores `pet-photos` y `business-photos` de inmediato.
4. **Avisar al equipo y al profesor el mismo día.** Un servicio caído sin aviso
   se interpreta como abandono del proyecto.

## Reactivar

Con crédito nuevo (renovación anual o suscripción de otro integrante):

```powershell
cd infra/azure
terraform init
terraform plan    # revisar que solo cree, nunca destruya
terraform apply
```

Después del apply, comprobar en este orden: `/api/v1/health/live`,
`/api/v1/health/ready` (verifica Mongo y Redis), y que haya **2 réplicas**:

```powershell
az containerapp replica list --name petmate-api --resource-group rg-petmate-prod --query "length(@)"
```

Si devuelve menos de 2, revisar que `MODO_AHORRO` esté en `false`.

## Quién decide

Los recortes 1 a 4 los aplica quien administra la suscripción (Adrián) sin
consultar: son reversibles y no cambian el diseño.

Bajar a una sola instancia, borrar datos de usuarios o migrar de proveedor
**son decisiones del equipo completo**, nunca de una persona, y se registran en
`memoria.md` con fecha y razón.
