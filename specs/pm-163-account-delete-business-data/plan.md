# Plan — pm-163-account-delete-business-data

## Diseño (pm-163#CA-01 a CA-03)
- Dentro de `AccountEraser`: si la cuenta tiene negocio, se borran sus registros de escaneo y las autorizaciones dirigidas a él (nuevos `DeleteByBusiness`) y después se ejecuta `DeleteBusinessHandler` de pm-110 (catálogo, foto pública, negocio e invalidación de la caché pública, CA-01 y CA-02).
- **CA-03:** las consultas, vacunas y desparasitaciones que el negocio registró en carnets ajenos no se tocan (solo se borran por mascota del dueño).
- Sin paquetes nuevos.

## Decisiones del agente a confirmar
- Los seguimientos y las conversaciones en que el negocio participaba como veterinaria **no** se borran: el dueño de la mascota conserva su historial; la cuenta de la veterinaria desaparece. El Spec no lo pide.
- Las autorizaciones «a cualquier negocio» no se tocan.

## Verificación
- Contrato `users-me-delete` (el negocio desaparece de la cuenta) y los `DeleteByBusiness` en los dobles de prueba.
