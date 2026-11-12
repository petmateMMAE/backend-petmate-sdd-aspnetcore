# Plan — pm-127-scan-ficha-veterinary

## Enfoque
- `POST scans/resolve` de una veterinaria devuelve, además de la ficha básica de `pm-128`, los grupos que alguna autorización vigente permite: `vaccines`, `dewormings`, `weights` y `consultations` (con su receta). Un grupo no autorizado no aparece en la respuesta (se omite, no viene vacío). Las alergias siempre van.
- Hasta 50 registros más recientes por grupo. Tienda y estética nunca reciben estos grupos aunque la autorización los incluya. El resultado es de solo lectura (no hay ruta de escritura asociada).
- Pruebas: contrato (`scans-resolve`) con autorización parcial y completa y con una tienda.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0).
