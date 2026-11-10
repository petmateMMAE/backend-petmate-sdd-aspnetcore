# Política de costo e infraestructura Azure

## Objetivo

Desplegar la demo de PetMate Backend en Azure sin generar costos: cuenta demo gratuita, servicios y herramientas con costo $0, y ninguna ampliación de plan no aprobada.

## Reglas

1. No crear un recurso Azure sin confirmar precio, cuota gratuita, duración del beneficio, límites de CPU/memoria/almacenamiento/egress y cargos por sobreuso.
2. No asumir que un plan históricamente gratuito continúa siéndolo.
3. Si una característica exige pago, no crearla; registrar la brecha y proponer una alternativa $0.
4. CI/CD debe poder compilar y probar sin desplegar. CD queda condicionado a confirmar costo $0.
5. Azure Key Vault se puede evaluar, pero no es obligatorio si genera costo; los secretos deben seguir fuera del repositorio y ser inyectados por entorno seguro.

## Arquitectura objetivo

- API en contenedor, stateless, capaz de escalar a varias instancias.
- Balanceador/ingress para múltiples réplicas cuando el plan $0 lo soporte.
- MongoDB Atlas como persistencia.
- Redis Auth independiente y Redis Cache independiente, accesibles por todas las réplicas.
- HTTPS público con certificado válido.
- Azure Blob Storage como origen de imágenes y CDN Azure para distribución.

## Compatibilidad con la regla de $0

La preparación horizontal es obligatoria. Sin embargo, si Azure no ofrece varias réplicas, balanceo, dos servicios Redis y CDN dentro de un costo total $0, la demo inicial no debe crear recursos pagados. En ese caso:

- Mantener contenedor, configuración stateless, redes y variables compatibles con varias réplicas.
- Desplegar solo el subconjunto que realmente pueda operar a $0, incluso una réplica si es el límite gratuito.
- Documentar expresamente cada capacidad diferida y su causa económica.
- No afirmar que existe alta disponibilidad/balanceo mientras no esté desplegado y verificado.

## Evaluación obligatoria antes del deployment

| Componente | Servicio Azure candidato | Cuota/costo confirmado | Límite | Alternativa $0 | Decisión |
|---|---|---|---|---|---|
| API container hosting | Container Apps, perfil Consumption, 2 réplicas fijas (0.25 vCPU / 0.5 GiB) | ~USD 9/mes tras la concesión gratuita (consulta 2026-11-05) | Crédito Azure for Students | No hay hosting de 2 réplicas a $0 | **Seleccionado** (ADR-003) |
| Load balancing / ingress | Ingress administrado del ambiente | Incluido | — | Es $0 | **Seleccionado** |
| Redis Auth | Container App `redis:7.4`, TCP interno, TLS, AOF en Azure Files | ~USD 5/mes | Crédito | Redis administrado excede presupuesto | **Seleccionado** (enmienda §8.9) |
| Redis Cache | Container App `redis:7.4`, TCP interno, TLS, LRU | ~USD 5/mes | Crédito | idem | **Seleccionado** (enmienda §8.9) |
| Blob Storage | — | — | — | — | Diferido: sin endpoints de imágenes (pm-079…082) |
| CDN | — | — | — | — | Diferido: sin endpoints de imágenes |
| TLS certificate | Certificado administrado de `*.azurecontainerapps.io` | Incluido | — | Es $0 | **Seleccionado** |
| Secret injection | Secretos de Container Apps (desde secretos de GitHub vía Terraform) | Incluido | — | Es $0 | **Seleccionado** (sin Key Vault) |

Total estimado ~USD 26–28/mes con ACR Basic y logs; desembolso directo $0 MXN (crédito de Azure for Students, sin tarjeta). Detalle y fuentes en `azure-deployment.md` §4; confirmar en la calculadora antes del primer `apply`.
La tabla debe completarse con fuentes oficiales vigentes al momento de planificar el despliegue. No se debe inventar precio ni disponibilidad. El estudio completo y vigente (2026-11-08) está en [azure-cost-study.md](azure-cost-study.md); las convenciones de entornos, nombres y etiquetas, en [environments.md](environments.md); y el índice de decisiones, en [../decisions/README.md](../decisions/README.md).
