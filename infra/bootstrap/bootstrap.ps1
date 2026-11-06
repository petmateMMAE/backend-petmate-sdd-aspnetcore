<#
.SYNOPSIS
  Paso único previo a Terraform (pm-064, pm-066, pm-067): verifica Azure for Students, crea el grupo de
  recursos de la app, el almacenamiento del estado de Terraform y la identidad OIDC de GitHub Actions.
.DESCRIPTION
  - No crea recursos con costo de cómputo: solo grupos de recursos, una cuenta de almacenamiento (centavos)
    y una aplicación de Entra ID sin secretos (federada con GitHub).
  - Requiere Azure CLI (`az login`) con la cuenta de Azure for Students.
  - Imprime los valores que se capturan como variables de GitHub. No imprime ningún secreto.
.EXAMPLE
  ./infra/bootstrap/bootstrap.ps1 -SubscriptionId <id> -Location chilecentral -CheckOnly
  ./infra/bootstrap/bootstrap.ps1 -SubscriptionId <id> -Location chilecentral
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$SubscriptionId,
    [string]$Location = 'chilecentral',
    [string]$AppResourceGroup = 'rg-petmate-prod',
    [string]$StateResourceGroup = 'rg-petmate-tfstate',
    [string]$GitHubRepository = 'petmateMMAE/backend-petmate-sdd-aspnetcore',
    [switch]$CheckOnly
)

$ErrorActionPreference = 'Stop'
function Invoke-AzCli { $out = & az.cmd @args; if ($LASTEXITCODE -ne 0) { throw "az $($args -join ' ') falló" }; return $out }

Invoke-AzCli account set --subscription $SubscriptionId | Out-Null
$accountName = Invoke-AzCli account show --query name -o tsv
Write-Host "Suscripción: $accountName" -ForegroundColor Cyan

# 1. Regiones permitidas por la política de Azure for Students.
Write-Host "`n== Regiones permitidas por política ==" -ForegroundColor Cyan
$policies = Invoke-AzCli policy assignment list --query "[].parameters" -o json | ConvertFrom-Json
$allowed = @()
foreach ($p in $policies) {
    if ($p -and $p.listOfAllowedLocations) { $allowed += $p.listOfAllowedLocations.value }
}
if ($allowed.Count -gt 0) {
    Write-Host ("Permitidas: " + (($allowed | Sort-Object -Unique) -join ', '))
    if ($Location -notin $allowed) { throw "La región '$Location' no está permitida. Usa una de la lista con -Location." }
} else {
    Write-Host 'No se encontró política de regiones; se usará la indicada.'
}

# 2. Container Apps disponible en la región.
$aca = Invoke-AzCli provider show --namespace Microsoft.App --query "resourceTypes[?resourceType=='managedEnvironments'].locations[]" -o tsv
$normalized = $aca | ForEach-Object { $_.ToLowerInvariant().Replace(' ', '') }
if ($Location -notin $normalized) { throw "Container Apps no está disponible en '$Location'." }
Write-Host "Container Apps disponible en $Location" -ForegroundColor Green

if ($CheckOnly) { Write-Host "`nSolo verificación: no se creó nada."; return }

# 3. Proveedores de recursos necesarios (sin costo).
foreach ($ns in 'Microsoft.App', 'Microsoft.OperationalInsights', 'Microsoft.ContainerRegistry', 'Microsoft.Storage',
    'Microsoft.Network', 'Microsoft.ManagedIdentity', 'Microsoft.Insights', 'Microsoft.Consumption') {
    Invoke-AzCli provider register --namespace $ns | Out-Null
}

# 4. Grupos de recursos: la app (Terraform mete todo adentro) y el estado (fuera del alcance del teardown).
$tags = @('proyecto=petmate', 'componente=backend', 'entorno=prod', 'costo=azure-for-students')
Invoke-AzCli group create -n $AppResourceGroup -l $Location --tags @tags | Out-Null
Invoke-AzCli group create -n $StateResourceGroup -l $Location --tags @tags | Out-Null
Write-Host "Grupos de recursos listos: $AppResourceGroup, $StateResourceGroup" -ForegroundColor Green

# 5. Almacenamiento del estado de Terraform (pm-067): cifrado, versionado, sin acceso público ni llaves compartidas.
$existing = Invoke-AzCli storage account list -g $StateResourceGroup --query "[?starts_with(name,'stpetmatetf')].name | [0]" -o tsv
$stateAccount = if ($existing) { $existing } else { 'stpetmatetf' + -join ((97..122) + (48..57) | Get-Random -Count 8 | ForEach-Object { [char]$_ }) }
if (-not $existing) {
    Invoke-AzCli storage account create -n $stateAccount -g $StateResourceGroup -l $Location --sku Standard_LRS --kind StorageV2 `
        --min-tls-version TLS1_2 --allow-blob-public-access false --allow-shared-key-access false --tags @tags | Out-Null
}
$stateId = Invoke-AzCli storage account show -n $stateAccount -g $StateResourceGroup --query id -o tsv
Invoke-AzCli storage account blob-service-properties update --account-name $stateAccount -g $StateResourceGroup --enable-versioning true | Out-Null
$me = Invoke-AzCli ad signed-in-user show --query id -o tsv
Invoke-AzCli role assignment create --assignee-object-id $me --assignee-principal-type User --role 'Storage Blob Data Contributor' --scope $stateId | Out-Null
Write-Host 'Esperando la propagación del rol (60 s)...'
Start-Sleep -Seconds 60
Invoke-AzCli storage container create --name tfstate --account-name $stateAccount --auth-mode login | Out-Null
Write-Host "Estado de Terraform: $stateAccount/tfstate" -ForegroundColor Green

# 6. Identidad de GitHub Actions por OIDC (sin secretos de cliente).
$appName = 'gh-petmate-backend-deploy'
$appId = Invoke-AzCli ad app list --display-name $appName --query '[0].appId' -o tsv
if (-not $appId) { $appId = Invoke-AzCli ad app create --display-name $appName --query appId -o tsv }
$spId = Invoke-AzCli ad sp list --filter "appId eq '$appId'" --query '[0].id' -o tsv
if (-not $spId) { $spId = Invoke-AzCli ad sp create --id $appId --query id -o tsv }

# La organización usa el plan gratuito de GitHub (sin environments en repos privados): Azure confía solo
# en los workflows que corren desde la rama main.
foreach ($envName in 'main') {
    $credName = "github-$envName"
    $has = Invoke-AzCli ad app federated-credential list --id $appId --query "[?name=='$credName'].name | [0]" -o tsv
    if (-not $has) {
        $body = @{ name = $credName; issuer = 'https://token.actions.githubusercontent.com';
            subject = "repo:${GitHubRepository}:ref:refs/heads/$envName"; audiences = @('api://AzureADTokenExchange') } | ConvertTo-Json -Compress
        # Nota: si la organización usa el formato de sujeto con IDs (repo:org@<id>/repo@<id>:...), registrar
        # también ese sujeto exacto (ver el error AADSTS700213 del primer run y docs/infrastructure/azure-deployment.md).
        $file = New-TemporaryFile
        [IO.File]::WriteAllText($file, $body)
        Invoke-AzCli ad app federated-credential create --id $appId --parameters "@$file" | Out-Null
        Remove-Item $file
    }
}

$appRgId = Invoke-AzCli group show -n $AppResourceGroup --query id -o tsv
foreach ($role in 'Contributor', 'Role Based Access Control Administrator') {
    Invoke-AzCli role assignment create --assignee-object-id $spId --assignee-principal-type ServicePrincipal --role $role --scope $appRgId | Out-Null
}
Invoke-AzCli role assignment create --assignee-object-id $spId --assignee-principal-type ServicePrincipal --role 'Storage Blob Data Contributor' --scope $stateId | Out-Null
$tenant = Invoke-AzCli account show --query tenantId -o tsv

Write-Host "`n== Captura en GitHub (Settings > Secrets and variables > Actions > Variables) ==" -ForegroundColor Cyan
Write-Host "AZURE_CLIENT_ID=$appId"
Write-Host "AZURE_TENANT_ID=$tenant"
Write-Host "AZURE_SUBSCRIPTION_ID=$SubscriptionId"
Write-Host "AZURE_LOCATION=$Location"
Write-Host "APP_RESOURCE_GROUP=$AppResourceGroup"
Write-Host "TFSTATE_RESOURCE_GROUP=$StateResourceGroup"
Write-Host "TFSTATE_STORAGE_ACCOUNT=$stateAccount"
Write-Host "`nSecretos (los capturas tú; este script no los conoce): TF_MONGODB_URI, TF_JWT_SIGNING_KEY, ALERT_EMAIL,"
Write-Host "ATLAS_PROJECT_ID, MONGODB_ATLAS_PUBLIC_KEY, MONGODB_ATLAS_PRIVATE_KEY, MONGODB_TEST_URI, MONGODB_TEST_DATABASE"
Write-Host "Los workflows de despliegue e infraestructura solo corren desde main."
