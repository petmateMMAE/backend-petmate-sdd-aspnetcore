# Gates — pm-029-user-registration

## Enmienda 2026-11-08 — T102 (H4)

> Estado: implementada y verificada; pendiente de revisión independiente y de aprobación humana. Aprobada por el usuario (2026-11-08).

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-029#CA-05` | T102 | Tiempos iguales en el registro y riesgo de enumeración aceptado y documentado | `RegisterHandler` (hash antes de `TryAdd`); `HardeningTests.Registering_a_duplicate_email_still_computes_the_password_hash...`; riesgo aceptado en `docs/api/endpoints.md` y `docs/security/asvs-l2-checklist.md` | Cumple |

| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 215, Security 180, Contract 202 (medidos al terminar esta Task; ver Memoria.md), Integration con los Redis desechables: 24 pasan y 4 se omiten del filtro Redis | Mismo contenedor; Redis con dos contenedores desechables de contraseñas aleatorias |
| Integridad SDD | OK | `scripts/check-sdd.ps1` (rango ampliado a T001–T102) |

- Limitaciones: El 409 sigue revelando que el correo existe; solo la verificación por correo lo elimina (spec futuro, requiere un servicio de correo y su costo).
- [ ] Revisión independiente.
- [x] Aprobación humana de T102 (2026-11-08).
