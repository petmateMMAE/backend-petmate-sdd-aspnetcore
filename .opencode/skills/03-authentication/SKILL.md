# Skill — Autenticación PetMate
Seguir los Specs `pm-022` a `pm-036` (`specs/pm-NNN-slug/`; historia en `specs/_historico/01-initial-authentication/`). Argon2id; JWT access de vida corta; refresh opaco con hash Atlas, rotación y replay detection; Logout revoca sesión en Atlas y jti en Redis Auth. Redis Auth y Redis Cache nunca se comparten. No crear Admin ni permitir autoasignación Business.
