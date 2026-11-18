# Gates — pm-073-public-https-endpoint (T085)

> Estado (2026-11-18): registro retrospectivo de lo que existe en el repositorio. **No cerrado.** Revisión estática solamente: no se ejecutó `terraform plan`/`apply` ni se consultó Azure, Atlas ni GitHub.

| CA | Evidencia | Resultado |
|---|---|---|
| `pm-073#CA-01` | Certificado administrado por Azure, válido. Comprobado en la prueba de humo de v0.1.0 (Memoria.md); no repetido | Parcial |
| `pm-073#CA-02` | `allow_insecure_connections = false` redirige HTTP a HTTPS; sin prueba registrada | Parcial |
| `pm-073#CA-03` | HSTS no está implementado en `src` ni en el ingress (brecha registrada en `azure-deployment.md`) | No cumple |
| `pm-073#CA-04` | `TRUSTED_PROXIES` está en `apps.tf`, pero según Memoria.md solo acepta IPs y la variable se dejó vacía hasta medirlas | Parcial |

## Razones para no aprobar aún
- CA-03: HSTS ausente. El propio documento de despliegue pide un spec para ello.
- CA-04: `TRUSTED_PROXIES` vacío por defecto; el rate limit agrupa por proxy.
- Sin revisión independiente ni aprobación humana (los agentes no se auto-firman).

## Cierre
- [ ] Criterios satisfechos con evidencia reproducible.
- [ ] Revisión independiente.
- [ ] Aprobación humana.
