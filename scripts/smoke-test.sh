#!/usr/bin/env bash
# Prueba de humo del despliegue (pm-077#CA-02, pm-084): verifica por HTTPS los 14 endpoints actuales.
# Uso: bash scripts/smoke-test.sh https://petmate-api.<dominio>
# Crea un usuario de prueba con correo aleatorio (no usa datos reales) y nunca imprime tokens.
set -euo pipefail

BASE="${1:?Uso: smoke-test.sh <url-https>}"
BASE="${BASE%/}"
case "$BASE" in https://*) ;; *) echo "La URL debe ser HTTPS"; exit 1 ;; esac

fail=0
check() { # nombre esperado obtenido
  if [ "$2" = "$3" ]; then echo "OK   $1 ($3)"; else echo "FALLO $1: esperado $2, obtenido $3"; fail=1; fi
}
req() { # método ruta [cuerpo] [token] -> imprime el código HTTP; el cuerpo queda en /tmp/smoke-body
  local args=(-sS -o /tmp/smoke-body -w '%{http_code}' -X "$1" "$BASE$2" -H 'Content-Type: application/json' --max-time 30)
  [ -n "${3:-}" ] && args+=(-d "$3")
  [ -n "${4:-}" ] && args+=(-H "Authorization: Bearer $4")
  curl "${args[@]}"
}

# Esperar a que la API esté lista (réplicas recién creadas).
for i in $(seq 1 30); do
  [ "$(req GET /api/v1/health/ready)" = "200" ] && break
  sleep 10
done

check "GET /api/v1/health/live" 200 "$(req GET /api/v1/health/live)"
check "GET /api/v1/health/ready" 200 "$(req GET /api/v1/health/ready)"
for dep in mongodb redis-auth redis-cache; do
  status=$(jq -r --arg d "$dep" '.data.dependencies[] | select(.name==$d) | .status' /tmp/smoke-body 2>/dev/null || true)
  check "  dependencia $dep" Healthy "$status"
done
check "GET /api/v1/technical/mediator-probe" 200 "$(req GET /api/v1/technical/mediator-probe)"

EMAIL="smoke-$(date +%s)-$RANDOM@example.com"
PASSWORD="Smk-$(openssl rand -hex 12)"
BODY=$(jq -nc --arg e "$EMAIL" --arg p "$PASSWORD" '{email:$e, password:$p}')

check "POST /api/v1/auth/register" 201 "$(req POST /api/v1/auth/register "$BODY")"
check "POST /api/v1/auth/login" 200 "$(req POST /api/v1/auth/login "$BODY")"
ACCESS=$(jq -r '.data.accessToken' /tmp/smoke-body)
REFRESH=$(jq -r '.data.refreshToken' /tmp/smoke-body)

check "POST /api/v1/auth/refresh" 200 "$(req POST /api/v1/auth/refresh "$(jq -nc --arg r "$REFRESH" '{refreshToken:$r}')")"
ACCESS=$(jq -r '.data.accessToken' /tmp/smoke-body)
REFRESH=$(jq -r '.data.refreshToken' /tmp/smoke-body)

# Perfil propio (pm-037, pm-038).
check "PUT /api/v1/users/me/update" 200 "$(req PUT /api/v1/users/me/update '{"displayName":"Prueba de humo"}' "$ACCESS")"
check "GET /api/v1/users/me/get" 200 "$(req GET /api/v1/users/me/get "" "$ACCESS")"
check "  el perfil devuelve el nombre" "Prueba de humo" "$(jq -r '.data.displayName' /tmp/smoke-body 2>/dev/null || true)"
check "GET /api/v1/users/me/get sin token" 401 "$(req GET /api/v1/users/me/get)"

# Mascotas del Owner (pm-040 a pm-044): crear, listar (paginado), ver, actualizar y eliminar (borrado físico).
PET='{"name":"Prueba de humo","species":"Perro","sex":"Unknown"}'
check "POST /api/v1/pets/create" 201 "$(req POST /api/v1/pets/create "$PET" "$ACCESS")"
PET_ID=$(jq -r '.data.id' /tmp/smoke-body 2>/dev/null || true)
check "GET /api/v1/pets/get-all" 200 "$(req GET "/api/v1/pets/get-all?page=1&pageSize=50" "" "$ACCESS")"
check "  el listado incluye la mascota creada" 1 "$(jq -r --arg id "$PET_ID" '[.data.items[] | select(.id==$id)] | length' /tmp/smoke-body 2>/dev/null || true)"
check "GET /api/v1/pets/get-all con pageSize inválido" 400 "$(req GET "/api/v1/pets/get-all?pageSize=51" "" "$ACCESS")"
check "GET /api/v1/pets/get-all sin token" 401 "$(req GET /api/v1/pets/get-all)"
check "GET /api/v1/pets/get-by-id/{id}" 200 "$(req GET "/api/v1/pets/get-by-id/$PET_ID" "" "$ACCESS")"
check "PUT /api/v1/pets/update/{id}" 200 "$(req PUT "/api/v1/pets/update/$PET_ID" '{"name":"Humo","species":"Gato","sex":"Female"}' "$ACCESS")"
check "POST /api/v1/pets/create sin token" 401 "$(req POST /api/v1/pets/create "$PET")"
check "DELETE /api/v1/pets/delete/{id}" 200 "$(req DELETE "/api/v1/pets/delete/$PET_ID" "" "$ACCESS")"
check "  la mascota eliminada ya no existe" 404 "$(req GET "/api/v1/pets/get-by-id/$PET_ID" "" "$ACCESS")"

check "POST /api/v1/auth/logout" 200 "$(req POST /api/v1/auth/logout "$(jq -nc --arg r "$REFRESH" '{refreshToken:$r}')" "$ACCESS")"
# El token revocado debe rechazarse en cualquier réplica (lista negra en Redis Auth compartido).
check "logout revoca el access token" 401 "$(req POST /api/v1/auth/logout "$(jq -nc --arg r "$REFRESH" '{refreshToken:$r}')" "$ACCESS")"

# HTTP debe redirigir a HTTPS.
HTTP_CODE=$(curl -s -o /dev/null -w '%{http_code}' "http://${BASE#https://}/api/v1/health/live" --max-time 30 || true)
case "$HTTP_CODE" in 301|302|307|308) echo "OK   HTTP redirige a HTTPS ($HTTP_CODE)";; *) echo "FALLO HTTP no redirige ($HTTP_CODE)"; fail=1;; esac

rm -f /tmp/smoke-body
[ "$fail" -eq 0 ] && echo "PRUEBA DE HUMO: CORRECTA" || { echo "PRUEBA DE HUMO: FALLÓ"; exit 1; }
