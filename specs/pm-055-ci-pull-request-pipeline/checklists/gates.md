# Gates — pm-055 ci-pull-request-pipeline (T066)

> Estado: T066 verificada el 2026-11-08; pendiente de revisión independiente y aprobación humana. T067 y T012 no se tocan.

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-055#CA-01` | `pipeline.yml` se dispara en `pull_request` (developer, release, main) y ejecuta secretos, build con analizadores, formato, CodeQL (informativo), NuGet, Trivy IaC, integridad SDD, suites unitaria, seguridad, contrato por endpoint y transversal, integración, imagen con Trivy y SBOM. Ningún paso de pago. | Cumple (revisión estática) |
| `pm-055#CA-02` | Paridad con `quality-gate.ps1`: restore, build `-warnaserror`, `dotnet format --verify-no-changes`, NuGet vulnerables, desuso y licencias (`-AuditOnly`), Unit, Security (incluye secret scan), Contract (matriz + transversal; `EndpointTraitTests` garantiza que ninguna prueba queda fuera) e Integration. | Cumple, con una diferencia documentada: el gate local bloquea vulnerabilidades desde `Low` y CI desde `High` (`-MinSeverity High`). Pendiente de que el usuario la apruebe o pida igualarla |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` y Windows PowerShell 5.1 local |
| Ejecución en GitHub | Última ejecución registrada en `Memoria.md` (`62dda53`, `main`) en verde con despliegue; no se volvió a ejecutar | El cambio de `pipeline.yml` todavía no se ejecutó en GitHub: no se ha hecho push (instrucción del usuario). La evidencia en CI llegará con la siguiente ejecución del pipeline. |

- Cambio menor: la fila de resumen "Pruebas por endpoint (9 jobs)" pasa a "(matriz)" porque la matriz ya tiene 13 endpoints.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T066.
