<#
.SYNOPSIS
  Aceptacion de extremo a extremo del primer hito (pm-046 / T041): recorre los 11 endpoints funcionales (4 de autenticacion y 7
  de perfil y mascotas) y las sondas, con casos felices, de validacion, de autorizacion y de aislamiento entre usuarios.
.DESCRIPTION
  Crea dos usuarios de prueba con correos aleatorios, no usa datos reales y no imprime tokens ni contrasenas.
  Respeta el limite por IP de autenticacion (10 por minuto): hace 10 llamadas a register/login/refresh; espere un minuto antes de repetirlo.
  Uso: powershell -File scripts/acceptance-milestone.ps1 [-Base http://127.0.0.1:8081/api/v1]
  Exit code 0 si todas las comprobaciones pasan.
#>
param([string]$Base = 'http://127.0.0.1:8081/api/v1')
$ErrorActionPreference = 'Stop'
$b = $Base
$script:fail = 0
$script:total = 0
function Call($method, $path, $body, $token) {
  $h = @{}; if ($token) { $h.Authorization = "Bearer $token" }
  try {
    $params = @{ UseBasicParsing = $true; Method = $method; Uri = "$b/$path"; Headers = $h; ContentType = 'application/json' }
    if ($null -ne $body) { $params.Body = ($body | ConvertTo-Json -Compress -Depth 5) }
    $r = Invoke-WebRequest @params
    [pscustomobject]@{ Code = [int]$r.StatusCode; Json = ($r.Content | ConvertFrom-Json) }
  } catch {
    $resp = $_.Exception.Response
    $json = $null
    if ($resp) { try { $sr = New-Object IO.StreamReader($resp.GetResponseStream()); $json = $sr.ReadToEnd() | ConvertFrom-Json } catch {} }
    [pscustomobject]@{ Code = [int]$resp.StatusCode; Json = $json }
  }
}
function Check($name, $expected, $actual) {
  $script:total++
  if ("$expected" -eq "$actual") { "OK    $name ($actual)" } else { $script:fail++; "FALLO $name : esperado $expected, obtenido $actual" }
}
$pw = 'Pw-' + [guid]::NewGuid().ToString('N')
$ana = "hito-ana-$([guid]::NewGuid().ToString('N').Substring(0,8))@example.com"
$luis = "hito-luis-$([guid]::NewGuid().ToString('N').Substring(0,8))@example.com"

"== Sondas =="
Check 'GET health/live' 200 (Call GET 'health/live').Code
Check 'GET health/ready' 200 (Call GET 'health/ready').Code

"== Autenticacion (4 endpoints) =="
$r = Call POST 'auth/register' @{ displayName = "Ana"; email = $ana; password = $pw }
Check 'POST auth/register' 201 $r.Code
Check '  rol Owner y sin contrasena en la respuesta' 'Owner' $r.Json.data.user.role
$regRaw = ($r.Json | ConvertTo-Json -Depth 6)
Check '  la respuesta no contiene la contrasena' $false ($regRaw -like "*$pw*")
Check 'POST auth/register (correo repetido)' 409 (Call POST 'auth/register' @{ displayName = "Ana"; email = $ana; password = $pw }).Code
Check 'POST auth/register (contrasena corta)' 400 (Call POST 'auth/register' @{ displayName = "Luis"; email = $luis; password = "corta" }).Code
$null = Call POST 'auth/register' @{ displayName = "Luis"; email = $luis; password = $pw }
$l = Call POST 'auth/login' @{ email = $ana; password = $pw }
Check 'POST auth/login' 200 $l.Code
$at = $l.Json.data.accessToken; $rt = $l.Json.data.refreshToken
Check 'POST auth/login (contrasena incorrecta)' 401 (Call POST 'auth/login' @{ email = $ana; password = 'incorrecta-123456' }).Code
$rf = Call POST 'auth/refresh' @{ refreshToken = $rt }
Check 'POST auth/refresh' 200 $rf.Code
$at2 = $rf.Json.data.accessToken; $rt2 = $rf.Json.data.refreshToken
Check 'POST auth/refresh (token invalido)' 401 (Call POST 'auth/refresh' @{ refreshToken = 'no-existe' }).Code
$ll = Call POST 'auth/login' @{ email = $luis; password = $pw }
$lat = $ll.Json.data.accessToken

"== Perfil propio (2 endpoints) =="
Check 'GET users/me/get' 200 (Call GET 'users/me/get' $null $at2).Code
Check 'PUT users/me/update' 200 (Call PUT 'users/me/update' @{ displayName = 'Ana Perez' } $at2).Code
Check '  el nombre quedo guardado' 'Ana Perez' (Call GET 'users/me/get' $null $at2).Json.data.displayName
Check 'PUT users/me/update (nombre vacio)' 400 (Call PUT 'users/me/update' @{ displayName = ' ' } $at2).Code
Check 'PUT users/me/update (rol no permitido)' 400 (Call PUT 'users/me/update' @{ displayName = 'X'; role = 'Business' } $at2).Code
Check 'GET users/me/get sin token' 401 (Call GET 'users/me/get').Code
Check '  cada usuario ve su propio correo' $luis (Call GET 'users/me/get' $null $lat).Json.data.email

"== Mascotas (5 endpoints) =="
$c = Call POST 'pets/create' @{ name = 'Firulais'; species = 'Perro'; breed = 'Mestizo'; birthDate = '2020-05-01'; sex = 'Male'; photoUrl = 'https://cdn.example.com/f.jpg' } $at2
Check 'POST pets/create' 201 $c.Code
$id = $c.Json.data.id
Check '  la respuesta no expone el propietario' $false (($c.Json | ConvertTo-Json -Depth 6) -like '*ownerId*')
Check 'POST pets/create (sexo invalido)' 400 (Call POST 'pets/create' @{ name = 'X'; species = 'Perro'; sex = 'Macho' } $at2).Code
Check 'POST pets/create (ownerId en el cuerpo)' 400 (Call POST 'pets/create' @{ name = 'X'; species = 'Perro'; sex = 'Male'; ownerId = [guid]::NewGuid() } $at2).Code
Check 'POST pets/create sin token' 401 (Call POST 'pets/create' @{ name = 'X'; species = 'Perro'; sex = 'Male' }).Code
$null = Call POST 'pets/create' @{ name = 'Michi'; species = 'Gato'; sex = 'Female' } $at2
$ga = Call GET 'pets/get-all?page=1&pageSize=1' $null $at2
Check 'GET pets/get-all' 200 $ga.Code
Check '  paginacion: 1 elemento de 2 en 2 paginas' '1/2/2' "$(@($ga.Json.data.items).Count)/$($ga.Json.data.totalCount)/$($ga.Json.data.totalPages)"
Check 'GET pets/get-all (pageSize 51)' 400 (Call GET 'pets/get-all?pageSize=51' $null $at2).Code
Check '  Luis no ve mascotas de Ana' 0 (Call GET 'pets/get-all' $null $lat).Json.data.totalCount
Check 'GET pets/get-by-id' 200 (Call GET "pets/get-by-id/$id" $null $at2).Code
Check '  mascota ajena (Luis) -> 404' 404 (Call GET "pets/get-by-id/$id" $null $lat).Code
Check 'PUT pets/update' 200 (Call PUT "pets/update/$id" @{ name = 'Max'; species = 'Perro'; sex = 'Male' } $at2).Code
Check '  actualizar mascota ajena -> 404' 404 (Call PUT "pets/update/$id" @{ name = 'Robada'; species = 'Perro'; sex = 'Male' } $lat).Code
Check '  eliminar mascota ajena -> 404' 404 (Call DELETE "pets/delete/$id" $null $lat).Code
Check '  la mascota sigue intacta' 'Max' (Call GET "pets/get-by-id/$id" $null $at2).Json.data.name
Check 'DELETE pets/delete' 200 (Call DELETE "pets/delete/$id" $null $at2).Code
Check '  borrado fisico: detalle -> 404' 404 (Call GET "pets/get-by-id/$id" $null $at2).Code

"== Cierre de sesion =="
Check 'POST auth/logout sin token' 401 (Call POST 'auth/logout' @{ refreshToken = $rt2 }).Code
Check 'POST auth/logout' 200 (Call POST 'auth/logout' @{ refreshToken = $rt2 } $at2).Code
Check '  access token revocado' 401 (Call GET 'users/me/get' $null $at2).Code
Check '  refresh token revocado' 401 (Call POST 'auth/refresh' @{ refreshToken = $rt2 }).Code

""
"RESULTADO: $($script:total - $script:fail) de $($script:total) comprobaciones correctas"
if ($script:fail -gt 0) { exit 1 }
