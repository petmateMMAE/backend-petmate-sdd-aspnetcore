# Gates — pm-077-cd-deployment-pipeline

> Estado (2026-11-19): revisado. Tres criterios cumplen; CA-04 queda limitado
> por el plan de GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-077#CA-01` | Cada imagen se etiqueta `VERSION-sha` y se bloquea en ACR. **Verificado en vivo** (`pm-074`): `0.3.0-239557a` tiene `writeEnabled: false` | **Cumple** |
| `pm-077#CA-02` | El job `smoke-test` prueba la API por HTTPS tras desplegar. El endpoint responde 200 en `/api/v1/health/live` y `/ready` | **Cumple** |
| `pm-077#CA-03` | Reversa automática a la revisión previa declarada en el workflow | **Cumple (estático)** |
| `pm-077#CA-04` | El despliegue sale con cada push a `main` sin aprobación de persona distinta. El plan gratuito de GitHub no permite *environments* con revisores | **Parcial** |

## Limitación registrada (CA-04)

No es un defecto del diseño del pipeline sino del plan contratado. El control
que sí existe es la protección de rama más el PR: nada llega a `main` sin pasar
por una revisión.

Si el profesor exige aprobación explícita de release, la salida es una regla de
protección de rama que pida revisión obligatoria de otra persona sobre `main`.

## Pendiente de evidencia (CA-03)

La reversa está declarada pero **nunca se ensayó**. Para cerrarlo hace falta
provocar un despliegue fallido a propósito y comprobar que vuelve sola a la
revisión anterior. Eso debe hacerse en una ventana acordada, no con la prueba
cerrada de Google Play en curso.

## Cierre

- [x] CA-01 y CA-02 satisfechos con evidencia verificada en vivo.
- [ ] CA-03 — declarado pero sin ensayo real.
- [ ] CA-04 — limitación del plan de GitHub.
- [ ] Aprobación humana: **no se firma** hasta ensayar la reversa.

## Nota

El ACR solo tiene imágenes 0.2.0 y 0.3.0: la 0.5.0 no se ha construido porque el
PR de `release` a `main` sigue abierto. El pipeline de despliegue no se ha
ejercitado desde el 2026-11-10.
