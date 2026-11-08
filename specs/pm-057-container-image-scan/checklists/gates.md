# Gates — pm-057 container-image-scan (T069)

> Estado: T069 verificada el 2026-11-08 con imagen chiseled y escaneo estricto; cerrada con aprobación humana el 2026-11-08 (sin revisión independiente).

| CA | Comprobación | Resultado |
|---|---|---|
| `pm-057#CA-01` | Trivy `aquasecurity/trivy-action@v0.35.0` escanea la imagen en `docker-build` (PR y main) y en `deploy` antes de publicarla. Herramienta $0. | Cumple |
| `pm-057#CA-02` | `severity: CRITICAL,HIGH` y `exit-code: '1'` sin `ignore-unfixed` ni `.trivyignore`: cualquier HIGH/CRITICAL bloquea; `deploy` depende de `docker-build`. | Cumple |

## Evidencia (2026-11-08, local con Trivy 0.58.1 y base de datos de vulnerabilidades del día)
| Imagen | HIGH/CRITICAL (con y sin parche) | Hallazgos totales | Tamaño |
|---|---|---|---|
| Anterior: `aspnet:10.0` + `apt-get upgrade` | 0 | 19 (12 MEDIUM, 7 LOW, todos sin parche: dash, libc, gnutls, perl, tar, systemd, login, icu) | 392 MB |
| Nueva: `aspnet:10.0-noble-chiseled` | 0 (`--exit-code 1` devuelve 0) | 2 MEDIUM sin parche en `libc6` (inevitables en cualquier imagen con glibc) | 225 MB |

| Verificación de funcionamiento | Resultado |
|---|---|
| `docker compose up` con la imagen chiseled y Atlas real | `/api/v1/health/ready` 200: mongodb (TLS), redis-auth y redis-cache `Healthy` |
| Flujo de punta a punta | register 201, login 200, perfil 200, mascota crear 201 / ver 200 / actualizar 200 / borrar 200 / ver tras borrar 404, refresh 200, logout 200, token revocado 401, sin token 401 |
| Sin shell en la imagen | `docker compose exec api sh` falla: ejecutable no encontrado |
| CA de Redis del CD | Build con `REDIS_CA_CERT_B64` de un certificado de prueba autofirmado: el certificado aparece en `/etc/ssl/certs/ca-certificates.crt` (121 certificados) |
| Pruebas de seguridad | 155/155 (contenedor SDK, incluye escaneo de secretos del repositorio) |
| GitHub | El pipeline con la imagen nueva aún no se ha ejecutado: no se ha hecho push (instrucción del usuario). El primer despliegue confirmará la conexión TLS a los Redis de Azure con la CA horneada. |

- [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella).
- [x] Aprobación humana del cierre de T069 (2026-11-08).
