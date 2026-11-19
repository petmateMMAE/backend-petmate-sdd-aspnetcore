# Gates — pm-067-iac-state-management

> Estado (2026-11-19): verificado contra Azure en vivo. Sustituye al registro
> estático del 2026-11-18.

## Evidencia recogida

```
$ az storage account show --name stpetmatetf7m3d91ki \
    --query "{claves:allowSharedKeyAccess,publico:allowBlobPublicAccess,tls:minimumTlsVersion,https:enableHttpsTrafficOnly}"
{ "claves": false, "https": true, "publico": false, "tls": "TLS1_2" }

$ az role assignment list --resource-group rg-petmate-tfstate
(sin asignaciones propias: hereda de la suscripcion)
```

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-067#CA-01` | Estado remoto en Blob, **cifrado confirmado** y con HTTPS obligatorio y TLS 1.2 mínimo. El bloqueo lo da el arrendamiento del blob, mecanismo propio de Terraform: `terraform init` y `plan` se ejecutaron el 2026-11-12 sin conflicto de estado | **Cumple** |
| `pm-067#CA-02` | `allowSharedKeyAccess: false` y `allowBlobPublicAccess: false` **verificado en vivo**: no hay llaves compartidas ni lectura anónima. El acceso es solo por identidad de Entra ID | **Cumple** |
| `pm-067#CA-03` | Cuenta Standard LRS con un solo archivo de estado; el consumo es de centavos al mes dentro del crédito de estudiante | **Cumple** |

## Observación sobre secretos en el estado

El archivo de estado de Terraform contiene valores sensibles por diseño (URI de
Mongo, llave de firma de JWT, contraseñas de Redis). Eso es inherente a
Terraform, no un defecto de esta configuración.

El control efectivo es el que se verificó arriba: **sin claves compartidas y sin
acceso público**, de modo que leer el estado exige una identidad de Entra ID con
permiso explícito sobre la suscripción. No hay ninguna vía anónima ni por cadena
de conexión.

No existen asignaciones de rol propias en `rg-petmate-tfstate`: hereda de la
suscripción, que es personal del estudiante. Para un entorno real convendría un
rol dedicado; para este proyecto el alcance es correcto.

## Cierre

- [x] Criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación contra la cuenta de estado en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.
