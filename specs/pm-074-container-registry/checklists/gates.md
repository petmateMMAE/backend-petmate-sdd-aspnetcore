# Gates — pm-074-container-registry (T086)

> Estado (2026-11-19): **verificado contra Azure en vivo**. Sustituye al registro
> estático del 2026-11-18.

## Evidencia recogida

```
$ az acr repository show --name acrpetmateprodd8u3u --image petmate-api:0.3.0-239557a \
    --query "{escribible:changeableAttributes.writeEnabled,borrable:changeableAttributes.deleteEnabled}"
{ "borrable": false, "escribible": false }

$ az acr repository show-tags --name acrpetmateprodd8u3u --repository petmate-api --orderby time_desc --top 5
0.3.0-239557a
0.3.0-f6967a1
0.3.0-b3b091b
0.3.0-9b10def
0.2.0-62dda53

$ az acr show --name acrpetmateprodd8u3u --query "{sku:sku.name,admin:adminUserEnabled}"
{ "admin": false, "sku": "Basic" }
```

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-074#CA-01` | ACR `acrpetmateprodd8u3u` activo, SKU Basic, con usuario administrador deshabilitado (acceso por identidad administrada) | **Cumple** |
| `pm-074#CA-02` | La etiqueta publicada tiene `writeEnabled: false` y `deleteEnabled: false`: **confirmado que el bloqueo se aplicó de verdad**, no solo que el pipeline lo intenta | **Cumple** |
| `pm-074#CA-03` | Cada etiqueta es `versión-SHA` (`0.3.0-239557a`): se rastrea del registro al commit exacto | **Cumple** |

## Cierre

- [x] Criterios satisfechos con evidencia reproducible.
- [x] Revisión independiente — verificación contra el registro en vivo.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Nota

El registro solo tiene imágenes 0.2.0 y 0.3.0. La 0.5.0 no se ha construido
porque el PR a `main` sigue abierto; eso no afecta a este Spec, pero sí a
`pm-077` y `pm-084`.
