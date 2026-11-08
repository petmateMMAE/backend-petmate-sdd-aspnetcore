# Validaciones y pruebas por endpoint

> Cada endpoint tiene su validación y sus pruebas de contrato. En el pipeline ([docs/ci-cd/pipeline.md](../ci-cd/pipeline.md)) cada fila de la tabla es un job independiente (`4 · Endpoint · <valor>`), todos al mismo nivel. El catálogo de rutas es [docs/api/endpoints.md](../api/endpoints.md).

## Cómo se ejecuta

- Cada método de `tests/PetMate.ContractTests` lleva `[Trait("Endpoint", "<valor>")]`. El job de un endpoint corre `dotnet test tests/PetMate.ContractTests --filter "Endpoint=<valor>"`.
- Las pruebas que no pertenecen a un solo endpoint (envoltorio de respuestas, OpenAPI/Scalar, JWT, políticas de autorización, superficie pública) llevan `Endpoint=transversal` y corren en el job `4 · Contrato transversal`.
- `EndpointTraitTests` (prueba de arquitectura) falla si un método de ContractTests no tiene Trait, si el valor no existe, si algún endpoint de la matriz se queda sin pruebas o si aparece una acción de controlador que no está en la matriz. Así ninguna prueba deja de ejecutarse en el pipeline sin que nadie lo note.
- Localmente: `dotnet test tests/PetMate.ContractTests --filter "Endpoint=auth-login"`.

## Matriz endpoint → validador → pruebas

| Valor del Trait | Endpoint | Acceso | Validación | Caso feliz | Entrada inválida → 400 con el wrapper | Sin token → 401 | Otros casos cubiertos |
|---|---|---|---|---|---|---|---|
| `auth-register` | `POST /api/v1/auth/register` | Público | `RegisterCommandValidator`: correo válido, contraseña de 12 a 128 caracteres; los cuerpos rechazan miembros desconocidos (`role`, `isAdmin`) | `Register_returns_201_wrapper_…` | `Register_returns_400_with_all_validation_errors_in_spanish`, `Register_rejects_any_attempt_to_send_a_role_…`, `Register_with_empty_or_malformed_body_…`, `Register_enforces_the_password_length_boundaries_of_12_to_128` | No aplica (público) | 409 con mensaje genérico por correo duplicado |
| `auth-login` | `POST /api/v1/auth/login` | Público | `LoginCommandValidator`: correo válido, contraseña obligatoria y de hasta 128 caracteres; cuota y bloqueo por correo (`AuthThrottlingEndpointsTests`) | `Login_succeeds_and_returns_a_working_access_token_…` | `Login_with_an_invalid_field_returns_400_wrapper_naming_the_field`, `Login_with_an_empty_malformed_or_extended_body_…` | No aplica (público) | 401 idéntico para correo inexistente y contraseña incorrecta; 429 con `Retry-After` |
| `auth-refresh` | `POST /api/v1/auth/refresh` | Público (refresh token) | `RefreshCommandValidator`: `refreshToken` obligatorio | `Refresh_rotates_and_the_old_token_cannot_be_reused_…`, `Expired_access_token_is_rejected_but_the_session_renews_…` | `Refresh_without_a_usable_token_returns_400_wrapper_naming_the_field`, `Refresh_with_an_empty_malformed_or_extended_body_…` | 401 con token desconocido (`Refresh_with_garbage_returns_401_wrapper`) | Replay de un token consumido revoca la sesión |
| `auth-logout` | `POST /api/v1/auth/logout` | Autenticado | `LogoutCommandValidator`: `refreshToken` obligatorio | `Logout_returns_200_with_null_data_then_both_tokens_stop_working` | `Logout_without_refresh_token_returns_400_…`, `Logout_with_a_blank_refresh_token_returns_400_wrapper_naming_the_field`, `Logout_with_an_empty_or_malformed_body_…` | `Logout_requires_a_valid_access_token` | 403 si la sesión es de otro usuario; 503 con Redis Auth caído (falla cerrada) |
| `users-me-get` | `GET /api/v1/users/me/get` | Autenticado | Sin cuerpo: validación equivalente en el controlador (la identidad sale solo del claim `sub`; sin él, 401) | `Get_returns_own_profile_without_secrets_…` | No aplica (no recibe entrada) | `Anonymous_requests_get_401_wrapper` | `Each_account_only_sees_its_own_profile`; 404 si la cuenta del token no existe |
| `users-me-update` | `PUT /api/v1/users/me/update` | Autenticado | `UpdateMyProfileCommandValidator`: nombre de 1 a 80 caracteres sin caracteres de control; el cuerpo rechaza `role` y `email` | `Update_sets_trimmed_name_and_get_returns_it` | `Update_with_missing_or_blank_name_…`, `Update_with_a_name_longer_than_80_returns_400`, `Update_rejects_attempts_to_change_role_or_email`, `Update_with_an_empty_or_malformed_body_returns_400_wrapper` | `Anonymous_requests_get_401_wrapper` | 404 si la cuenta del token no existe y no se crea nada |
| `health-live` | `GET /api/v1/health/live` | Público | Sin entrada; solo acepta GET | `Live_returns_200_wrapper_without_checking_dependencies` | No aplica | No aplica (público, sondas del balanceador) | `Live_rejects_non_get_methods_with_405_wrapper` |
| `health-ready` | `GET /api/v1/health/ready` | Público | Sin entrada; solo acepta GET | `Ready_with_healthy_dependencies_returns_200_wrapper_…` | No aplica | No aplica (público) | 503 que nombra cada dependencia no disponible sin exponer secretos; 405 para otros métodos |
| `pets-create` | `POST /api/v1/pets/create` | Owner | `CreatePetCommandValidator` (`PetValidation`): nombre, especie, raza, fecha de nacimiento, sexo y foto; el cuerpo rechaza `ownerId`, `id` y fechas; `PetLimits` (100 por propietario), límite de 60 por minuto y 16 KB de cuerpo (`PetLimitsTests`) | `Create_returns_201_wrapper_…` | `Create_with_an_invalid_field_…`, `Create_rejects_owner_id_or_server_fields_…`, `Create_with_an_empty_or_malformed_body_…` | `Create_requires_an_owner_token` (también 403 Business) | El propietario sale del token |
| `pets-get-all` | `GET /api/v1/pets/get-all` | Owner | `ListMyPetsQueryValidator` (`PaginationConvention`): `page` ≥ 1 y `pageSize` de 1 a 50; el propietario sale del token | `Get_all_returns_the_standard_page_shape_with_defaults_and_only_the_own_pets`, `Get_all_pages_through_the_pets_…`, `Get_all_accepts_the_maximum_page_size_of_50` | `Get_all_with_an_invalid_parameter_returns_400_naming_it` | `Get_all_requires_an_owner_token` (también 403 Business) | Lista vacía, página fuera de rango, `ownerId` en la consulta ignorado |
| `pets-get-by-id` | `GET /api/v1/pets/get-by-id/{id}` | Owner + propietario | Sin cuerpo: `{id}` debe ser GUID (si no, 404) y el propietario sale del token | `Get_by_id_returns_the_owner_pet` | No aplica | `Get_by_id_requires_an_owner_token` (también 403 Business) | 404 para mascota ajena, inexistente o id mal formado |
| `pets-update` | `PUT /api/v1/pets/update/{id}` | Owner + propietario | `UpdateMyPetCommandValidator` (mismas reglas que crear) | `Update_replaces_the_editable_fields_…` | `Update_with_an_invalid_field_…`, `Update_rejects_an_owner_id_in_the_body` | `Update_requires_an_owner_token` | 404 y sin cambios para mascota ajena o inexistente |
| `pets-delete` | `DELETE /api/v1/pets/delete/{id}` | Owner + propietario | Sin cuerpo: `{id}` GUID y propietario del token | `Delete_removes_the_owner_pet_physically_…` | No aplica | `Delete_requires_an_owner_token` | 404 y nada borrado para mascota ajena; borrado físico |
| `technical-mediator-probe` | `GET /api/v1/technical/mediator-probe` | Público (temporal, ver pm-033) | Sin entrada; los parámetros de la consulta se ignoran | `Get_returns_200_wrapper_with_the_mediated_result` | No aplica | No aplica (público temporal) | `Query_string_parameters_are_ignored`, `Other_http_methods_return_405_wrapper`, `Only_the_versioned_technical_route_is_mapped` |

Nota: `Anonymous_requests_get_401_wrapper` es una prueba parametrizada que cubre ambos endpoints de perfil, por eso lleva los dos Trait.

## Cobertura transversal (`Endpoint=transversal`)

| Tema | Clases |
|---|---|
| JWT: sin token, expirado, firma o audiencia ajena, revocado, Redis caído (503) | `JwtPipelineTests` |
| Políticas de autorización (`RequireOwner`, `RequireBusiness`), 401/403 con wrapper, denegación por defecto | `AuthorizationPolicyTests` |
| Claims manipulados: rol desconocido o ausente, arreglo, firma de otra clave, payload editado, `alg: none` | `ForgedClaimsTests` |
| Superficie pública cerrada (Development y Production) | `PublicSurfaceTests` |
| Response Wrapper: 400, 404, 405, 500 y `data: null` | `ResponseWrapperHttpTests` |
| OpenAPI y Scalar (solo Development) | `OpenApiEndpointsTests` |
| Trait obligatorio en cada prueba y matriz de endpoints | `EndpointTraitTests` |

## Totales de la última ejecución (contenedor SDK)

| Valor | Pruebas |
|---|---:|
| `auth-register` | 14 |
| `auth-login` | 10 |
| `auth-refresh` | 9 |
| `auth-logout` | 8 |
| `users-me-get` | 5 |
| `users-me-update` | 12 |
| `health-live` | 2 |
| `health-ready` | 3 |
| `technical-mediator-probe` | 6 |
| `transversal` | 49 |

La tabla refleja la primera ejecución con los endpoints de autenticación, perfil y salud (116 pruebas); hoy ContractTests suma 157 e incluye los endpoints `pets-*`, cuyos casos están en el `checklists/gates.md` de cada Spec de mascotas. La suma de la tabla supera el total porque las dos pruebas de `Anonymous_requests_get_401_wrapper` cuentan en los dos endpoints de perfil.

## Cómo agregar un endpoint nuevo

1. Spec aprobado y fila en `docs/api/endpoints.md`.
2. Su `IRequestValidator` (o validación equivalente en el controlador si no recibe cuerpo).
3. Pruebas de contrato con `[Trait("Endpoint", "<valor-nuevo>")]`: caso feliz, entrada inválida (400 con wrapper) y sin token (401) donde aplique.
4. Agregar el valor a la matriz de `endpoint-tests` en `.github/workflows/pipeline.yml`, a `EndpointNames` y a `RouteToEndpoint` en `EndpointTraitTests`, y una fila en este documento.
