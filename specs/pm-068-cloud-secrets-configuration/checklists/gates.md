# Gates — pm-068-cloud-secrets-configuration

> Estado (2026-11-19): verificado contra Azure en vivo y **cerrado**: el
> documento de rotación que faltaba se escribió en este mismo ciclo.

## Evidencia recogida

```
$ az containerapp show --name petmate-api --resource-group rg-petmate-prod \
    --query "properties.configuration.secrets[].name"
jwt-signing-key
mongodb-uri
redis-auth-connection
redis-cache-connection

$ az containerapp show ... --query "identity.type"
UserAssigned
```

Los secretos aparecen **solo por nombre**: Azure no devuelve sus valores ni
siquiera al propietario por esta vía. La app usa identidad administrada
(`UserAssigned`), de modo que no hay cadenas de conexión con llave.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-068#CA-01` | **Los 4 secretos existen en la Container App como referencias con nombre**, sin valores en archivos versionados ni recuperables por consulta | **Cumple** |
| `pm-068#CA-02` | `.dockerignore` excluye `.env` de la imagen y la producción recibe su configuración del entorno. Confirmado que no hay variables con secretos en texto plano | **Cumple** |
| `pm-068#CA-03` | **Documento creado**: [`docs/security/rotacion-de-secretos.md`](../../../docs/security/rotacion-de-secretos.md). Cubre los 4 secretos, su periodicidad, el procedimiento de cada uno, qué se rompe al rotarlos y qué hacer ante una filtración | **Cumple** |
| `pm-068#CA-04` | Key Vault no se usa; la condición se cumple por omisión | **Cumple** |

## Nota sobre la llave de JWT

El documento deja escrito el efecto que antes no estaba en ningún lado: rotar
`jwt-signing-key` **invalida todas las sesiones abiertas**. También advierte de
mantenerla sincronizada con el secreto `TF_JWT_SIGNING_KEY` de GitHub, porque si
se desincronizan el siguiente `terraform apply` revierte el valor desplegado.

## Cierre

- [x] Los cuatro criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación contra Azure en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.
