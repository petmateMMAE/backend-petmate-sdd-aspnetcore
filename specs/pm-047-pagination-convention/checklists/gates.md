# Gates — pm-047 pagination-convention (T058)

> Estado: T058 implementada y verificada el 2026-11-08; pendiente de aprobación humana.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-047#CA-01` | T058 | `PetListTests.The_convention_uses_the_approved_values`, `Out_of_range_values_are_rejected_naming_the_parameter`; `PetListEndpointsTests.Get_all_accepts_the_maximum_page_size_of_50`, `Get_all_returns_the_standard_page_shape_with_defaults_…` (defaults 1 y 20) | En verde |
| `pm-047#CA-02` | T058 | `PetListEndpointsTests.Get_all_returns_the_standard_page_shape_…` (claves exactas `items`, `page`, `pageSize`, `totalCount`, `totalPages`), `Get_all_pages_through_the_pets_…`, `Get_all_with_no_pets_returns_an_empty_page`; `PetListTests.Total_pages_rounds_up` | En verde |
| `pm-047#CA-03` | T058 | `PetListEndpointsTests.Get_all_with_an_invalid_parameter_returns_400_naming_it` (9 casos: 0, negativo, texto, decimal, 51, desbordamiento); `PetListTests.Both_invalid_parameters_are_reported_together` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (repositorio de solo lectura, copia sin `.env`), Atlas de pruebas real y dos Redis temporales con contraseñas aleatorias |
| Suites | Unit 172/172, Security 162/162, Contract 157/157, Integration 49/49 (0 omitidas) | Mismo contenedor |
| Extremo a extremo | `docker compose` con la imagen chiseled y Atlas real: página 1 con 2 de 3, página 2 con 1, `pageSize=51`/`page=0`/`page=abc` → 400, sin token → 401 | `Invoke-WebRequest` local |
| Paquetes nuevos | Ninguno | — |

- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T058.
