# Gates — pm-060 sdd-integrity-gate (T072)

> Estado: T072 implementada y verificada localmente el 2026-11-08; pendiente de ejecución en CI, revisión independiente y aprobación humana.

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-060#CA-01` | IDs consecutivos, dependencias existentes, sin autodependencia ni ciclos (Kahn), una Task con un solo Spec principal (nuevo), CA sin duplicados y consecutivos (nuevo), un solo propietario por CA. | Cumple |
| `pm-060#CA-02` | Trazabilidad completa (nuevo): cada criterio de cada Spec tiene fila en `traceability.md` con su Task propietaria y no hay filas de criterios inexistentes. Corre en el job bloqueante `3 · Integridad SDD`. | Cumple |
| `pm-060#CA-03` | `scripts/check-sdd.tests.ps1`: 8 casos (la copia intacta pasa; CA duplicado, Task con dos Specs principales, criterio sin fila, fila de criterio inexistente, propietario equivocado, ciclo y documento histórico alterado fallan con su mensaje). | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| `check-sdd.ps1` sobre el repositorio | OK: 84 Specs, 97 Tasks, 34 históricos SHA-256 | Windows PowerShell 5.1 |
| `check-sdd.tests.ps1` | 8/8 casos correctos | Windows PowerShell 5.1 |
| CI | Paso nuevo "Pruebas negativas del gate SDD" en `sdd-integrity` | El cambio de `pipeline.yml` todavía no se ejecutó en GitHub: no se ha hecho push (instrucción del usuario). La evidencia en CI llegará con la siguiente ejecución del pipeline. |

- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T072.
