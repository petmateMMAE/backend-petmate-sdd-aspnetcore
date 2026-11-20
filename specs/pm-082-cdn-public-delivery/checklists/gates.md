# Gates — pm-082-cdn-public-delivery (T094)

> Estado (2026-11-19): **diferido por decisión de arquitectura registrada**
> (ADR-007), no por olvido. No se crea ningún recurso de CDN.

## La decisión ya estaba tomada

El [ADR-007](../../../docs/decisions/ADR-007-cdn.md) la registró antes de que
existieran los endpoints de imágenes:

> **Diferida.** Cuando haya contenido público, evaluar **Azure CDN (Microsoft)**
> o **Azure Front Door** con el precio y el crédito vigentes. Clasificación:
> **Posible cargo — diferido**; Front Door tiene base de ~35 USD/mes, lo que
> excede el presupuesto de la demo. **No se crea ningún recurso.**

El presupuesto total es de 40 USD y ya sostiene la API encendida 24/7 desde el
2026-11-17. Front Door por sí solo lo consumiría casi entero.

## Qué se hace en su lugar

Las imágenes públicas se sirven **directo desde Blob Storage**, como `storage.tf`
documenta:

```hcl
# El CDN (pm-082) no está aprobado: las imágenes públicas se sirven directo del
# Blob, como previó ADR-007.
```

El contenedor `business-photos` es público de solo lectura
(`container_access_type = "blob"`), así que la foto del local se ve en el mapa
sin necesidad de cuenta. Las fotos de mascotas siguen en `pet-photos`, privado,
y solo se leen con URL temporal firmada (`pm-081`).

## Los criterios, en el estado actual

| CA | Criterio | Estado |
|---|---|---|
| `pm-082#CA-01` | El CDN distribuye únicamente contenido público | **No aplica** — no hay CDN |
| `pm-082#CA-02` | El origen privado no queda expuesto por el CDN | **Se cumple por diseño** — sin CDN no hay forma de exponerlo, y `pet-photos` es privado en el origen |

CA-02 merece una aclaración: el riesgo que el criterio quiere evitar es que un
CDN mal configurado cachee y publique contenido privado. Sin CDN ese riesgo no
existe, y además la separación en dos contenedores con permisos distintos lo
previene desde el origen.

## Qué se pierde

Latencia. Las fotos se sirven desde Chile Central; un usuario lejos de esa
región las carga más lento que con un CDN delante.

No se pierde funcionalidad: las imágenes cargan, son públicas las que deben
serlo y privadas las que deben serlo.

## Cierre

- [ ] CA-01 — no aplica mientras no exista CDN.
- [x] CA-02 — se cumple por diseño del almacenamiento.
- [x] Decisión registrada en ADR-007, con el costo que la motiva.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Cuándo retomarlo

Si el proyecto consigue presupuesto o sale de Azure for Students, evaluar Azure
CDN estándar (cobro por uso) antes que Front Door. Agregarlo después no obliga a
cambiar la aplicación: la API devuelve la URL de la foto, y esa URL puede pasar a
apuntar al CDN sin tocar el cliente.
