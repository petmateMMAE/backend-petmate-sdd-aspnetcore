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

## Por qué no se ensayó la reversa (2026-11-19)

Se intentó y no se pudo, por una razón concreta:

```
$ az containerapp revision list --name petmate-api --resource-group rg-petmate-prod
petmate-api--0000009   petmate-api:0.5.0-77760e1   activa   100% del trafico
```

**Hay una sola revisión activa.** Las anteriores ya se retiraron, así que no hay
a dónde revertir: desactivar la única que sirve dejaría la API caída, con los
testers de Google Play usándola.

La reversa está declarada en el workflow y el mecanismo existe. Lo que falta es
la prueba, y necesita dos cosas:

1. Que haya al menos dos revisiones activas, lo cual ocurre tras el siguiente
   despliegue.
2. Una ventana acordada con el equipo, fuera del horario de los testers.

Mientras tanto queda registrado como pendiente, no como defecto.
