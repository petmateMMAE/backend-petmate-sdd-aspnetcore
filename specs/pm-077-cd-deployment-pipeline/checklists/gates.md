# Gates — pm-077-cd-deployment-pipeline

> Estado (2026-11-19): **cerrado**. La reversa se ensayó de verdad contra
> producción, en una ventana acordada con el equipo y con los 12 testers
> avisados.

## CA-03 — Ensayo de la reversa

El registro anterior dejó este criterio sin evidencia: la reversa estaba
declarada en el workflow pero nunca se había ejecutado. Hoy se ensayó.

### Cómo

Se simuló un despliegue fallido desplegando la imagen anterior (`0.3.0`), que no
tiene los endpoints de imágenes, y se comprobó que la prueba de humo lo detecta
y que la reversa recupera el servicio.

**Estado inicial**

```
revisión: petmate-api--0000009
imagen:   petmate-api:0.5.0-77760e1
live 200 · ready 200 · upload-photo 401
```

**Paso 1 — despliegue "fallido"**

```
$ az containerapp update -n petmate-api -g rg-petmate-prod \
    --image acrpetmateprodd8u3u.azurecr.io/petmate-api:0.3.0-239557a
petmate-api--0000010   petmate-api:0.3.0-239557a
```

**Paso 2 — la prueba de humo detecta la regresión**

| Ruta | Antes | Con la revisión mala |
|---|---|---|
| `/health/live` | 200 | 200 |
| `POST /pets/upload-photo/{id}` | 401 | **404** |

`live` sigue en 200 —la API arranca— pero `upload-photo` devuelve **404**: la
0.3.0 no tiene ese endpoint. Es exactamente el tipo de regresión que una prueba
de humo debe atrapar y que un chequeo de salud por sí solo dejaría pasar.

**Paso 3 — reversa, con el mismo comando del workflow**

```
$ az containerapp update -n petmate-api -g rg-petmate-prod \
    --image acrpetmateprodd8u3u.azurecr.io/petmate-api:0.5.0-77760e1
petmate-api--0000011   petmate-api:0.5.0-77760e1
```

**Paso 4 — servicio recuperado**

```
live:          200
ready:         200   (verifica Mongo y Redis)
upload-photo:  401   (de vuelta desde 404)
```

```
$ az containerapp revision list
petmate-api--0000010   0.3.0-239557a   0 %     Deprovisioning
petmate-api--0000011   0.5.0-77760e1   100 %   RunningAtMaxScale

$ az containerapp replica list --query "length(@)"
2
```

La revisión mala se retira sola, la buena toma todo el tráfico y vuelven las dos
réplicas.

### Lo que el ensayo demuestra

1. La prueba de humo distingue "la API arranca" de "la API sirve": `live` nunca
   dejó de responder 200 y aun así el despliegue era malo.
2. El comando de reversa del workflow funciona contra producción real.
3. El modo de revisión `Single` hace el cambio sin ventana de caída: la revisión
   nueva se activa antes de que la anterior se retire.

## Los cuatro criterios

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-077#CA-01` | Etiqueta `VERSION-sha` bloqueada en ACR; verificado en `pm-074` que `writeEnabled: false` | **Cumple** |
| `pm-077#CA-02` | Prueba de humo por HTTPS tras desplegar; el despliegue a `main` del 2026-11-20 pasó | **Cumple** |
| `pm-077#CA-03` | **Reversa ensayada contra producción**, con la regresión detectada y el servicio recuperado | **Cumple** |
| `pm-077#CA-04` | El despliegue sale con cada push a `main` sin aprobación de persona distinta: el plan gratuito de GitHub no permite *environments* con revisores | **Parcial** |

### CA-04, limitación del plan

No es un defecto del diseño del pipeline sino del plan contratado. El control
que sí existe es la protección de rama más el PR: nada llega a `main` sin pasar
por revisión.

Si el profesor exige aprobación explícita de release, la salida es una regla de
protección de rama que pida revisión obligatoria de otra persona sobre `main`.

## Cierre

- [x] CA-01, CA-02 y CA-03 satisfechos con evidencia reproducible.
- [ ] CA-04 — limitación del plan gratuito de GitHub, documentada.
- [x] Revisión independiente — ensayo ejecutado contra producción, no simulado.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Condiciones del ensayo

Se hizo con el equipo avisado y los 12 testers de Google Play notificados de que
el servicio se interrumpiría un rato. Fuera de esa ventana, este ensayo **no
debe repetirse** en producción: implica desplegar a propósito una versión que se
sabe mala.
