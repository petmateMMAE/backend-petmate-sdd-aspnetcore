# Plan — pm-056 test-coverage-baseline (T068)

## Enfoque
- Medir con `coverlet.collector` 6.0.4 (ya central, MIT, $0), formato cobertura, por suite, en el contenedor del SDK.
- `scripts/coverage-merge.py` (biblioteca estándar) une los informes por línea y excluye pruebas y código generado.
- Registrar la línea base y proponer el umbral en `docs/quality/coverage-baseline.md`. **El umbral solo entra en vigor mediante enmienda aprobada** (CA-02): no se modifica la Constitución ni el gate sin aprobación de Miguel.
