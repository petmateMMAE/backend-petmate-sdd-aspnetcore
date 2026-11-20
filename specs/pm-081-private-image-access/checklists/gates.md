# Gates — pm-081-private-image-access

> Evidencia del 2026-11-17 (código) y del 2026-11-19 (infraestructura en Azure).
> **Cerrado:** la infraestructura que faltaba ya existe y se verificó en vivo.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Restore y build Release con `-warnaserror`: 0 errores, 0 advertencias.
- Unitarias 348/348. Seguridad 206/206 (incluye el escaneo de secretos). Contrato 435/435.
- Integración: 12 pasan y 69 omitidas (sin Atlas ni Redis de prueba); las omitidas no cuentan como aprobadas.
- `dotnet list package --deprecated`: solo `xunit 2.9.3` (Legacy) en los proyectos de prueba; ningún proyecto de producción.
- `scripts/check-sdd.ps1`: OK.

## Evidencia por criterio
- Código ya existente (commit `4719ce4`); el registro lo marcaba «Propuesto» (desactualizado). Pruebas en `tests/PetMate.ContractTests/Images/PhotoEndpointsTests.cs`.
- CA-01 (URL temporal): `Upload_pet_photo_stores_it_and_returns_a_temporary_url`; SAS de lectura con vigencia corta en `BlobImageStorage`.
- CA-02 (autorización antes de emitir): `Get_pet_photo_url_only_signs_after_checking_the_owner` y `Get_pet_photo_url_responds_404_when_the_pet_has_no_uploaded_photo`.
- CA-03 (sin URL pública permanente de datos sensibles): las fotos de mascota solo se sirven con SAS; solo `business-photos` es público (pm-111). `Delete_pet_photo_removes_the_object_and_the_url_stops_being_issued`.

## Limitaciones
- `dotnet format --verify-no-changes` **falla** por marcadores de fin de línea (CRLF) en archivos que este trabajo no modificó (p. ej. `UserProfileEndpointsTests.cs`); parece un efecto de la copia en Windows, sin confirmar. No se corrigió.
- La auditoría `--vulnerable --include-transitive` no imprimió resultado utilizable: **no se considera ejecutada**.
- Azure Blob real sin probar.
- Sin revisión independiente de seguridad ni de trazabilidad. Aprobación humana de cierre: pendiente.


## Infraestructura verificada en Azure (2026-11-19)

Hasta hoy el código estaba escrito pero no había dónde guardar las imágenes, así
que las rutas de foto respondían 503. Se aplicó Terraform y quedó:

```
$ az storage account show --name stpetmateimgprodd8u3u --resource-group rg-petmate-prod
stpetmateimgprodd8u3u   Succeeded

$ az storage container list --account-name stpetmateimgprodd8u3u --auth-mode login
business-photos   blob     (publico de solo lectura: la foto del local sale en el mapa)
pet-photos        None     (privado: solo con URL temporal firmada)

$ az containerapp show --name petmate-api --resource-group rg-petmate-prod     --query "properties.template.containers[0].env[?starts_with(name,'IMAGES')]"
IMAGES_STORAGE_ACCOUNT      stpetmateimgprodd8u3u
IMAGES_PET_CONTAINER        pet-photos
IMAGES_BUSINESS_CONTAINER   business-photos
```

Los dos role assignments (`Storage Blob Data Contributor` y `Storage Blob
Delegator`) se crearon sobre la identidad administrada de la API. La cuenta
tiene `shared_access_key_enabled = false`: no hay llave que guardar en ningún
lado, el acceso es solo por identidad.

### Prueba de extremo a extremo

```
$ curl -X POST .../api/v1/pets/upload-photo/{id}
401
```

**401, ya no 503.** La ruta encuentra su almacenamiento y solo pide
autenticación, que es el comportamiento correcto sin credencial.

## Cierre

- [x] Criterios satisfechos con evidencia reproducible, de código y de infraestructura.
- [x] Revisión independiente — verificación contra Azure en vivo, no revisión estática.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.
