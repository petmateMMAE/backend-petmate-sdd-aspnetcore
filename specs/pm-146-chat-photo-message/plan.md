# Plan — pm-146-chat-photo-message

## Estado
- **No implementado.** Depende de `pm-079` (adaptador de almacenamiento de imágenes en Azure Blob), que sigue «Propuesto»: Blob tiene costo y necesita autorización expresa y la verificación de su costo $0 (`AGENTS.md` §4). Un mensaje con foto necesita el contenedor privado y las URL temporales de `pm-081`.
- Cuando se apruebe Blob: mensaje con foto de hasta 1 MB y tipo de imagen permitido, privada con URL temporal para los participantes y borrada con la conversación (`pm-143`).
