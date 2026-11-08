# Línea base de rendimiento (pm-053 / T064)

> Medida el 2026-11-08 con `PerformanceBaselineTests` (ContractTests): prueba de carga **en proceso**, costo $0, sin red ni servicios externos, con repositorios en memoria y datos sintéticos (sin PII). Concurrencia 16, 600 solicitudes por endpoint tras 50 de calentamiento, en el contenedor del SDK.

## Objetivos de latencia (CA-01)
| Endpoint | Objetivo p95 (en proceso) |
|---|---|
| `GET /api/v1/health/live` | ≤ 250 ms |
| `GET /api/v1/pets/get-all` (20 y 10 elementos) | ≤ 250 ms |

Los topes son holgados a propósito para no ser frágiles en CI; los valores medidos están muy por debajo. Objetivos para el despliegue (con red y Atlas, a medir en pm-084): p95 ≤ 500 ms en lecturas y ≤ 1500 ms en login (Argon2id).

## Resultados (CA-02, CA-03)
| Endpoint | p50 | p95 | p99 | Throughput | Errores |
|---|---|---|---|---|---|
| `health/live` | 4.3 ms | 10.1 ms | 20.1 ms | 2894 req/s | 0 |
| `pets/get-all?pageSize=20` | 9.1 ms | 19.9 ms | 37.7 ms | 1521 req/s | 0 |
| `pets/get-all?page=2&pageSize=10` | 6.7 ms | 13.2 ms | 17.4 ms | 2257 req/s | 0 |

## Reproducir
`dotnet test tests/PetMate.ContractTests --filter FullyQualifiedName~PerformanceBaselineTests --logger "console;verbosity=detailed"` y buscar las líneas `MEDICION`. Resultados sin datos personales ni secretos.

## Limitaciones
No incluye red, TLS, Atlas ni Redis reales (la latencia real será mayor); no mide login (Argon2id depende de los núcleos); los números varían con la máquina. La prueba de carga contra el despliegue en Azure queda para pm-084.
