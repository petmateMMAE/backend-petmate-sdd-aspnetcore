# ADR-002 — OpenAPI y Scalar

## Decisión

El usuario aprueba la candidata C para T007: `Microsoft.AspNetCore.OpenApi` 10.0.12, `Microsoft.OpenApi` 2.12.0 y `Scalar.AspNetCore` 2.11.2. La combinación se incorpora exclusivamente para OpenAPI y Scalar tras auditoría limpia, compatibilidad comprobada con `net10.0` y verificación en proyecto temporal.

## Auditoría de dependencias (2026-11-03)

- La combinación anterior `Microsoft.AspNetCore.OpenApi` 10.0.0 → `Microsoft.OpenApi` 2.0.0 se rechaza: NuGet Audit informa `GHSA-v5pm-xwqc-g5wc`/`CVE-2026-49451`, severidad alta. El advisory afecta OpenAPI.NET `>=2.0.0-preview.11,<2.7.5` y `>=3.0.0,<3.5.4`; corrige desde 2.7.5 y 3.5.4.
- `Microsoft.AspNetCore.OpenApi` 10.0.12 declara para `net10.0` la dependencia `[2.12.0, 3.0.0)` sobre `Microsoft.OpenApi`; por ello resuelve 2.12.0, una versión corregida en la línea 2.x compatible con ASP.NET Core 10.
- `Scalar.AspNetCore` 2.11.2 declara MIT, `net10.0`, costo $0 y ninguna dependencia NuGet; utiliza solo el framework `Microsoft.AspNetCore.App`.
- En proyectos temporales `net10.0`, la combinación aprobada restauró, compiló y pasó `dotnet list package --vulnerable --include-transitive` sin hallazgos. En Development, `/openapi/v1.json` y `/scalar/v1` respondieron HTTP 200; en Production permanecieron ocultos por defecto.
- Fuentes: advisory oficial `https://github.com/microsoft/OpenAPI.NET/security/advisories/GHSA-v5pm-xwqc-g5wc`; nuspecs oficiales de `https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.openapi/10.0.12/microsoft.aspnetcore.openapi.nuspec` y `https://api.nuget.org/v3-flatcontainer/scalar.aspnetcore/2.11.2/scalar.aspnetcore.nuspec`; Microsoft Learn sobre OpenAPI ASP.NET Core 10.

## Seguridad y límites

- OpenAPI y Scalar se habilitan en Development. En Production quedan deshabilitados por defecto y solo podrán habilitarse mediante configuración explícita revisada.
- Scalar es la única UI interactiva; no se agregan Swagger UI, Swashbuckle UI, NSwag UI ni otra UI.
- La dependencia `Microsoft.OpenApi` se declara explícitamente para evitar que la resolución vuelva a una versión vulnerable o incompatible con la línea 10.x.
- No se agregan endpoints de negocio, wrapper, autenticación, persistencia ni tareas posteriores.
