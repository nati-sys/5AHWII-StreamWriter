# Einloggen, wenn nötig
az account show 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
  az login
}

# Location suchen, wo ein App Service Plan deployed werden kann
$Location = @(az policy assignment list --query "[?name == 'sys.regionrestriction'].parameters.listOfAllowedLocations.value[]" | ConvertFrom-Json)[0]
if (-not $Location) {
  $Location = @(az account list-locations --query "[?metadata.geographyGroup == 'Europe'].name" | ConvertFrom-Json)[0]
}

$SubscriptionId = az account show --query id --output tsv
$UserId = $SubscriptionId.Substring(0, 8)

$ResourceGroup = 'rg-streamwriter'
$EnvironmentName = 'cae-streamwriter'
$ContainerAppName = "ca-streamwriter-$UserId"

az extension add --name containerapp --upgrade --only-show-errors

az group create --name $ResourceGroup --location $Location | Out-Null

# Container App Environment als Rahmen für Container Apps anlegen
az containerapp env create `
  --resource-group $ResourceGroup `
  --name $EnvironmentName `
  --location $Location | Out-Null

# Container App mit Platzhalter-Image anlegen
az containerapp create `
  --resource-group $ResourceGroup `
  --name $ContainerAppName `
  --environment $EnvironmentName `
  --image mcr.microsoft.com/dotnet/samples:aspnetapp-10.0 `
  --target-port 8080 `
  --ingress external `
  --min-replicas 0 `
  --max-replicas 1 `
  --system-assigned | Out-Null

# Fully qualified domain name
$Fqdn = az containerapp show `
  --resource-group $ResourceGroup `
  --name $ContainerAppName `
  --query properties.configuration.ingress.fqdn `
  --output tsv
Write-Host "=== Container app running at https://$Fqdn"

<#
az group delete --name rg-streamwriter --yes --no-wait 
#>
