# Gates — pm-046 first-milestone-acceptance (T041)

> Estado: validación de extremo a extremo realizada y en verde el 2026-11-08. **Hito aprobado explícitamente por el usuario el 2026-11-08; T041 y pm-046 `Cerrado`.**

| CA | Task | Evidencia | Resultado |
|---|---|---|---|
| `pm-046#CA-01` | T041 | `scripts/acceptance-milestone.ps1` contra `docker compose` (imagen chiseled, Atlas y Redis reales): **39 de 39** comprobaciones; suites Unit 188/188, Security 170/170, Contract 170/170, Integration 56/56; NuGet y escaneo de imagen en verde (abajo) | En verde |
| `pm-046#CA-02` | T041 | Aprobación humana explícita del hito (2026-11-08) | Cumplido |

## Cobertura por endpoint del primer hito
| # | Endpoint | Comprobado en la validación |
|---|---|---|
| 1 | `POST auth/register` | 201 con rol Owner y sin contraseña en la respuesta; 409 correo repetido; 400 contraseña corta |
| 2 | `POST auth/login` | 200; 401 contraseña incorrecta |
| 3 | `POST auth/refresh` | 200 con rotación; 401 token inválido |
| 4 | `POST auth/logout` | 401 sin token; 200; el access token y el refresh token quedan revocados (401) |
| 5 | `GET users/me/get` | 200; 401 sin token; cada usuario ve solo su propio correo |
| 6 | `PUT users/me/update` | 200 y el nombre queda guardado; 400 nombre vacío; 400 rol no permitido |
| 7 | `POST pets/create` | 201 sin exponer el propietario; 400 sexo inválido; 400 `ownerId` en el cuerpo; 401 sin token |
| 8 | `GET pets/get-all` | 200 paginado (1 de 2 elementos, 2 páginas); 400 con `pageSize` 51; el otro usuario ve 0 |
| 9 | `GET pets/get-by-id` | 200 al propietario; 404 al otro usuario |
| 10 | `PUT pets/update` | 200; 404 al otro usuario y la mascota queda intacta |
| 11 | `DELETE pets/delete` | 404 al otro usuario; 200 al propietario; el detalle posterior da 404 (borrado físico) |
| — | `GET health/live`, `GET health/ready` | 200 (Atlas, Redis de autenticación y Redis de caché sanos) |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Aceptación de extremo a extremo | 39/39 | `scripts/acceptance-milestone.ps1` con la imagen `petmate-api:local` (225 MB, chiseled), Atlas real y Redis de Compose |
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` |
| Suites | Unit 188/188, Security 170/170, Contract 170/170, Integration 56/56 (Atlas real y Redis temporales, 0 omitidas) | Mismo contenedor |
| Auditoría NuGet | vulnerabilidades (desde High), desuso y 50 licencias en verde | `scripts/quality-gate.ps1 -AuditOnly` |
| Escaneo de la imagen | 0 HIGH/CRITICAL (Trivy 0.58.1, sin ignorar hallazgos sin parche) | `petmate-api:local` |
| Integridad SDD | OK | `scripts/check-sdd.ps1` y `scripts/check-sdd.tests.ps1` |
| Revisiones independientes | Módulo de mascotas y grupo de autenticación, 2026-11-08: apto con observaciones y consistente con observaciones | Ver pm-045 y pm-035 gates |
| Paquetes nuevos | Ninguno | — |

## Limitaciones y asuntos abiertos para el hito
- El pipeline de GitHub aún no ha ejecutado los cambios de imagen chiseled, límites y gates recientes (no hay pull request abierto); el despliegue en Azure con esta versión es aparte.
- Hallazgos de seguridad abiertos y sin decisión: H3, H4, H6, H7, H8 e I2 (ver [pm-034 gates](../../pm-034-auth-brute-force-protection/checklists/gates.md)).
- Observaciones menores: el desempate por `_id` del listado no está en el índice (pm-052); el CORS y los endpoints de salud no tienen evaluación propia en este hito.
- Specs base todavía «En revisión» (por ejemplo pm-011, pm-013, pm-024 a pm-027).
- [ ] Revisión independiente específica de T041 (no realizada; el hito se apoya en las del módulo de mascotas y del grupo de autenticación, y el usuario lo aprobó sin ella).
- [x] Aprobación humana explícita del hito (CA-02, 2026-11-08).
