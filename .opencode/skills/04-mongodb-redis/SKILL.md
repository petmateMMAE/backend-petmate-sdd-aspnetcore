# Skill — Persistencia Atlas y Redis
MongoDB Atlas es el único motor de datos, también para integration tests (DB de test separada). Prohibido Mongo local/container y cualquier DB alternativa. Redis Auth y Redis Cache son dos servicios independientes, con configuración/credenciales separadas. No usar cache como fuente de verdad.
