# Rotación de secretos

> Spec: `pm-068` (CA-03) · Aprobado por Adrián Adonai García Jiménez, 2026-11-19

Qué secretos tiene el sistema, cada cuánto se cambian, cómo se cambian y qué se
rompe al hacerlo.

## Los secretos

Viven en la Container App `petmate-api` del grupo `rg-petmate-prod`. Se
consultan por nombre; Azure no devuelve sus valores por consulta.

| Secreto | Qué protege | Rotación | Riesgo al rotar |
|---|---|---|---|
| `jwt-signing-key` | Firma de los tokens de acceso | Cada 6 meses o ante sospecha | **Alto**: invalida todas las sesiones abiertas |
| `mongodb-uri` | Credenciales de Atlas | Cada 6 meses o al salir alguien del equipo | Medio: corta la base mientras se propaga |
| `redis-auth-connection` | Redis de revocación de tokens | Cada 6 meses | Medio: la API falla cerrado y devuelve 503 |
| `redis-cache-connection` | Redis de caché | Cada 6 meses | Bajo: la caché se degrada, la API sigue |

Fuera de Azure hay dos más: `TF_MONGODB_URI` y `TF_JWT_SIGNING_KEY`, secretos de
GitHub que usa Terraform. **Tienen que coincidir con los de Azure**: si se
desincronizan, el siguiente `terraform apply` sobrescribe el valor desplegado.

## Rotar la llave de firma de JWT

La más delicada. Al cambiarla, **todos los usuarios con sesión abierta son
expulsados** y tienen que volver a entrar. Sus datos no se pierden, pero es una
interrupción visible.

No rotarla durante la prueba cerrada de Google Play ni el día de una
evaluación.

```powershell
# 1. Generar la llave nueva
$nueva = openssl rand -base64 48

# 2. Actualizarla en Azure
az containerapp secret set --name petmate-api --resource-group rg-petmate-prod `
  --secrets jwt-signing-key=$nueva

# 3. Reiniciar para que la tome
az containerapp revision restart --name petmate-api --resource-group rg-petmate-prod `
  --revision (az containerapp show --name petmate-api --resource-group rg-petmate-prod `
    --query "properties.latestRevisionName" -o tsv)

# 4. Actualizar el secreto de GitHub para que Terraform no la revierta
gh secret set TF_JWT_SIGNING_KEY --repo petmateMMAE/backend-petmate-sdd-aspnetcore --body $nueva
```

El paso 4 no es opcional: si se olvida, el siguiente `apply` devuelve la llave
vieja y vuelve a expulsar a todos.

## Rotar la URI de Mongo

1. En Atlas, crear un usuario nuevo con los mismos privilegios (solo la base
   `petmate_prod`, sin permisos de administración).
2. Actualizar `mongodb-uri` en Azure y `TF_MONGODB_URI` en GitHub.
3. Reiniciar la revisión.
4. Comprobar `/api/v1/health/ready`, que verifica la conexión.
5. **Recién entonces** borrar el usuario viejo en Atlas.

Crear antes de borrar: así la API nunca queda sin credencial válida.

## Rotar las conexiones de Redis

Redis corre como Container App dentro de la red privada, no como servicio
administrado. La contraseña se genera en Terraform (`random_password`), así que
rotarla es forzar su regeneración:

```powershell
cd infra/azure
terraform taint random_password.redis_auth
terraform apply
```

El Redis de autenticación falla cerrado: mientras se propaga el cambio, la API
responde 503 en vez de dejar pasar tokens sin verificar. Es el comportamiento
correcto, pero significa unos segundos de indisponibilidad.

## Si un secreto se filtra

1. Rotarlo **de inmediato**, sin esperar la ventana programada.
2. Si es `jwt-signing-key`, además revocar las sesiones en Redis Auth.
3. Revisar los registros de Log Analytics en busca de accesos anómalos.
4. Anotar el incidente en `memoria.md` con fecha, qué se filtró y cómo.

El pipeline corre gitleaks en cada push, así que un secreto que llegue al
repositorio rompe la compilación antes de mergearse. Eso cubre la filtración
por commit, no la que ocurre por compartir un valor en un chat.

## Calendario

| Cuándo | Qué |
|---|---|
| Cada 6 meses | Los cuatro secretos, en ventana acordada |
| Al salir alguien del equipo | `mongodb-uri` y los secretos de GitHub |
| Ante sospecha de filtración | El secreto afectado, de inmediato |
| **Nunca** | Durante la prueba cerrada de Play o el día de evaluación |
