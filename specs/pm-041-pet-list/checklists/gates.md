# Gates — pm-041 pet-list (T031)

> Estado: T031 implementada y verificada el 2026-11-08; pendiente de aprobación humana.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-041#CA-01` | T031 | `PetListEndpointsTests.Get_all_returns_the_standard_page_shape_with_defaults_and_only_the_own_pets`, `Get_all_ignores_an_owner_id_sent_in_the_query_string`, `Get_all_requires_an_owner_token` (401/403); `PetListTests.List_returns_only_the_owner_pets_newest_first_…`, `A_user_without_pets_gets_an_empty_page_…`; `PetPersistenceIntegrationTests.List_returns_only_the_owner_pets_newest_first_with_paging` y `Count_by_owner_counts_only_the_owner_pets_and_follows_deletes` (Atlas) | En verde |
| `pm-041#CA-02` | T031 | `PetListEndpointsTests.Get_all_pages_through_the_pets_…`, `…_accepts_the_maximum_page_size_of_50`, `…_invalid_parameter_returns_400_naming_it`; `PetListTests.List_pages_without_repeating_or_losing_pets`, `A_page_beyond_the_last_is_empty_and_never_overflows` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (repositorio de solo lectura, copia sin `.env`), Atlas de pruebas real y dos Redis temporales con contraseñas aleatorias |
| Suites | Unit 172/172, Security 162/162, Contract 157/157, Integration 49/49 (0 omitidas) | Mismo contenedor |
| Consulta con índice | `Owner_listing_query_is_served_by_the_owner_index` (IXSCAN) | Atlas de pruebas |
| Extremo a extremo | Imagen chiseled y Atlas real: cada usuario solo ve sus mascotas (Ana 3, Luis 1) | `Invoke-WebRequest` local |

- Observación: el desempate por `_id` del orden no está en el índice (se ordena en memoria dentro del propietario); revisar en pm-052.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T031.
