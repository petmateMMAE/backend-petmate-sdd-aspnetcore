# Estudio de costos de Azure for Students (pm-062 / T074)

> Fecha de consulta: **2026-11-08**. Este estudio solo **lee** páginas oficiales y calcula: no crea recursos reales ni habilita servicios con posibles cargos (pm-062 CA-05). Los precios de lista cambian: confirmar en la [calculadora de Azure](https://azure.microsoft.com/pricing/calculator/) antes de cualquier `apply`. Ningún servicio se asume gratuito por pertenecer a Azure for Students ni por haberlo sido antes.
> Decisiones que usan este estudio: `docs/decisions/ADR-003-azure-container-apps.md` y `ADR-005` a `ADR-010`. Política: `azure-cost-policy.md`. Despliegue: `azure-deployment.md`.

## 1. Supuestos

| Dato | Valor | Fuente (consulta 2026-11-08) |
|---|---|---|
| Beneficio | 100 USD de crédito, utilizable dentro de 12 meses; sin tarjeta de crédito | [Azure for Students](https://azure.microsoft.com/free/students/) |
| Tipo de cambio de referencia | 1 USD = 18.652374 MXN (actualización de la fuente: 2026-11-08 00:02 UTC) | [open.er-api.com](https://open.er-api.com/v6/latest/USD) |
| Crédito en MXN | 100 USD ≈ **1 865 MXN** | Cálculo |
| Moneda de la constitución | MXN | Constitución §8 |

Los importes en USD de los componentes con precio variable son los estimados de lista del primer estudio (consulta 2026-11-05, `azure-deployment.md` §4); donde la página de precios no cargó se indica y queda pendiente de la calculadora. **No se inventa ningún precio.**

## 2. Componentes: fuente, clasificación y costo estimado (pm-062 CA-01 y CA-02)

Clasificación (constitución §8.2): **Incluido** = sin cargo adicional; **Crédito** = cubierto por el crédito de Azure for Students, con importe; **Posible cargo** = podría generar cargo adicional.

| # | Componente | Servicio seleccionado | Fuente oficial (URL) | Estado de la fuente (2026-11-08) | Clasificación | USD/mes | MXN/mes |
|---|---|---|---|---|---|---:|---:|
| 1 | Cómputo de la API (2 instancias) | Azure Container Apps, Consumption, 2 réplicas de 0.25 vCPU / 0.5 GiB | [Facturación](https://learn.microsoft.com/azure/container-apps/billing) · [Precios](https://azure.microsoft.com/pricing/details/container-apps/) | Verificada: gratis cada mes por suscripción 180 000 vCPU-s, 360 000 GiB-s y 2 millones de solicitudes; después se paga por consumo. Los precios por segundo no cargaron: se conserva el estimado del 2026-11-05 | Crédito | ~9 | ~168 |
| 2 | Load Balancer | Ingress administrado de Container Apps | [Ingress](https://learn.microsoft.com/azure/container-apps/ingress-overview) | Verificada: al habilitar ingress no hace falta crear un Azure Load Balancer ni una IP pública; reparte entre réplicas | Incluido | 0 | 0 |
| 3 | Red y salida de datos | VNet con subred delegada; tráfico saliente a Internet | [Ancho de banda](https://azure.microsoft.com/pricing/details/bandwidth/) | Verificada: entrada gratis y **los primeros 100 GB/mes de salida gratis**; la API (JSON) queda muy por debajo | Incluido (dentro de la franja gratuita) | 0 | 0 |
| 4 | DNS y certificados | Dominio `*.azurecontainerapps.io` y certificado TLS administrado en el ingress | [Ingress](https://learn.microsoft.com/azure/container-apps/ingress-overview) | Verificada: HTTPS siempre con TLS 1.2/1.3 terminado en el ingress, con FQDN por defecto y redirección 80→443 | Incluido | 0 | 0 |
| 5 | Registro de contenedores | Azure Container Registry Basic (10 GB incluidos) | [Precios ACR](https://azure.microsoft.com/pricing/details/container-registry/) | Parcial: confirma 10 GB y 2 webhooks; el precio diario no cargó. ACR figura entre los servicios gratuitos 12 meses de la oferta de estudiantes. Se conserva ~5 USD hasta confirmarlo en la calculadora | Crédito (a confirmar) | ~5 | ~93 |
| 6 | Almacenamiento (estado de IaC y AOF de Redis Auth) | Blob (estado de Terraform) y Azure Files 1 GiB | [Precios Files](https://azure.microsoft.com/pricing/details/storage/files/) · [Precios Blob](https://azure.microsoft.com/pricing/details/storage/blobs/) | Parcial: los precios no cargaron (página dinámica). Cantidades mínimas (1 GiB y megabytes) | Crédito | < 1 | < 19 |
| 7 | Almacenamiento de imágenes de mascotas | Azure Blob Storage privado | [Precios Blob](https://azure.microsoft.com/pricing/details/storage/blobs/) | No aplica: **diferido**, no se crea (pm-079 a pm-082 sin implementar); se evaluará con precio vigente antes de crearlo | Posible cargo (diferido) | 0 (no se crea) | 0 |
| 8 | CDN | Azure CDN / Front Door | [Precios CDN](https://azure.microsoft.com/pricing/details/cdn/) · [Front Door](https://azure.microsoft.com/pricing/details/frontdoor/) | No aplica: **diferido**, no se crea (sin endpoints de imágenes). Front Door tiene base de ~35 USD/mes (estudio del 2026-11-05) | Posible cargo (diferido) | 0 (no se crea) | 0 |
| 9 | Redis Auth | Container App con `redis:7.4`, TCP interno, TLS, AOF | [Facturación Container Apps](https://learn.microsoft.com/azure/container-apps/billing) | Verificada la base de cobro (consumo por segundo); estimado del 2026-11-05 | Crédito | ~5 | ~93 |
| 10 | Redis Cache | Container App con `redis:7.4`, TCP interno, TLS, LRU | idem | idem | Crédito | ~5 | ~93 |
| 11 | Observabilidad | Log Analytics (tope diario de ingesta 0.15 GB) y alerta de 5xx | [Costos de Azure Monitor Logs](https://learn.microsoft.com/azure/azure-monitor/logs/cost-logs) · [Precios Monitor](https://azure.microsoft.com/pricing/details/monitor/) | Verificada: se factura por GB ingerido y retención (precio regional) y existe tope diario; no se confirmó una franja gratuita fija, así que no se supone | Crédito | ~1 | ~19 |
| 12 | Secretos | Secretos de Container Apps (sin Key Vault) | [Secretos](https://learn.microsoft.com/azure/container-apps/manage-secrets) | URL oficial; **no se consultó en esta pasada**: se conserva la conclusión del estudio del 2026-11-05 (sin cargo adicional). Key Vault no se usa (pm-068 CA-04) | Incluido | 0 | 0 |
| 13 | Estado de IaC | Blob con bloqueo y cifrado | [Precios Blob](https://azure.microsoft.com/pricing/details/storage/blobs/) | Incluido en la fila 6 (megabytes) | Crédito | < 1 | < 19 |
| 14 | CI/CD y escaneos | GitHub Actions (repositorio privado, minutos gratuitos), gitleaks, Trivy, Syft, CodeQL | [GitHub Actions](https://docs.github.com/billing/managing-billing-for-your-products/managing-billing-for-github-actions/about-billing-for-github-actions) | URL oficial; **no se consultó en esta pasada**. Herramientas de código abierto y minutos gratuitos del plan (no son de Azure) | Incluido | 0 | 0 |
| 15 | Tráfico hacia Atlas | MongoDB Atlas M0 desde las réplicas, por Internet con lista de IPs | [Ancho de banda](https://azure.microsoft.com/pricing/details/bandwidth/) · [Atlas](https://www.mongodb.com/pricing) | Verificada la franja gratuita de salida (los primeros 100 GB/mes); la página de Atlas **no se consultó en esta pasada** (servicio externo M0 gratuito según el estudio del 2026-11-05) | Incluido | 0 | 0 |
| — | Presupuesto y alertas | Cost Management | [Cost Management](https://learn.microsoft.com/azure/cost-management-billing/costs/) | Verificada: figura entre los servicios gratuitos de la oferta de estudiantes (página de Azure for Students) | Incluido | 0 | 0 |
| | **Total (24 h, 2 réplicas fijas)** | | | | | **~26–28** | **~485–522** |

## 3. Costo total de operación y consumo de créditos (CA-03)

| Escenario | Costo mensual | Cobertura del crédito de 100 USD (~1 865 MXN) |
|---|---|---|
| **Completo** (evaluación: 2 réplicas fijas y Redis 24 h) | ~26–28 USD (~485–522 MXN) | Dura **~3.6–3.8 meses** (100 ÷ 27) |
| **Modo ahorro** (API de 0 a 2 réplicas por peticiones y Redis de 7:00 a 23:00) | ~13–15 USD (~242–280 MXN) | Dura **~6.7–7.7 meses** |
| **Ventana de evaluación** del 2026-11-06 (primer despliegue) al 2026-12-02 (26 días en modo completo) | ~23–24 USD (~420–455 MXN) | Consume ~23 % del crédito |

El consumo **real** acumulado debe leerse en Cost Management de la suscripción; este estudio no consulta Azure (CA-05). El presupuesto de Terraform (40 USD/mes) avisa al 50, 80 y 100 %.

## 4. Conflicto académico y requisitos que no se reducen (CA-04)

- Mantener la arquitectura completa **12 meses** cuesta ~324 USD, más que el crédito (100 USD). No es posible sostenerla todo el año sin desembolso adicional.
- **No se reduce ningún requisito:** las dos instancias de la API y el Load Balancer se mantienen mientras la demo está activa. Lo que se acota es el **tiempo de operación**: la evaluación final (2026-12-02) cabe en el crédito (arriba), y después se aplica el procedimiento de pm-065 (agotamiento de créditos sin recurrir a una sola instancia) o el `destroy` de pm-083.
- Al no haber tarjeta asociada, no existe método de pago al que cobrar. El comportamiento exacto al agotarse el crédito debe confirmarse en la FAQ de educación de Microsoft (la URL consultada el 2026-11-08 devolvió 404); hasta entonces, el presupuesto y las alertas son el control.
- Blob Storage y CDN (imágenes) están **diferidos** porque tienen posible cargo; se evaluarán con el precio vigente y el crédito restante antes de crearlos.

## 5. Observaciones del ingress relevantes para seguridad
Según la documentación de Container Apps (consulta 2026-11-08), `X-Forwarded-For` trae **solo la IP más a la derecha** y cualquier otro valor debe validarlo el usuario para evitar la falsificación; el backend usa `ForwardLimit = 1` y solo confía en los proxies listados en `TRUSTED_PROXIES` (pm-017, con evidencia en `ForwardedHeadersSpoofingTests`).

## 6. Pendientes de este estudio
- Confirmar en la calculadora los precios que no cargaron (ACR Basic, Azure Files, Blob, Container Apps por segundo) y registrar el resultado aquí.
- Leer el consumo real en Cost Management y compararlo con la sección 3.
