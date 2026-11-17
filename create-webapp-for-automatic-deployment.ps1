# Einloggen, wenn nötig
# 2>&1 means errors werden nicht ausgegeben 
az account show 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
  az login
}

gh auth status 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
  gh auth login
}

# Location suchen, wo ein App Service Plan deployed werden kann
$Location = @(az policy assignment list --query "[?name == 'sys.regionrestriction'].parameters.listOfAllowedLocations.value[]" | ConvertFrom-Json)[0]
if (-not $Location) {
  $Location = @(az account list-locations --query "[?metadata.geographyGroup == 'Europe'].name" | ConvertFrom-Json)[0]
}

# Ressourcennamen festlegen
$SubscriptionId = az account show --query id --output tsv
$UserId = $SubscriptionId.Substring(0, 8)
$ResourceGroup = 'rg-streamwriter'
$EnvironmentName = 'cae-streamwriter'
$ContainerAppName = 'ca-streamwriter'
$RegistryName = "crstreamwriter$UserId"
$GitHubIdentityName = 'id-streamwriter-github'

# GitHub-Repository (owner/repo) und Branch, von dem aus deployed wird, festlegen
$GitHubRepo = gh repo view --json nameWithOwner --jq .nameWithOwner 2>$null
if (-not $GitHubRepo) { throw 'GitHub-Repository nicht gefunden.' }
$GitHubBranch = 'main'

az extension add --name containerapp --upgrade --only-show-errors

az group create --name $ResourceGroup --location $Location | Out-Null

# Azure Container Registry für die Images anlegen
$Registry = az acr create `
  --resource-group $ResourceGroup `
  --name $RegistryName `
  --location $Location `
  --sku Basic | ConvertFrom-Json
$RegistryLoginServer = $Registry.loginServer

# Container App Environment als Rahmen für Container Apps anlegen
# --environment-mode WorkloadProfiles: Standard-Modus statt "Express", weil Express-CAEs
# keinen Image-Pull aus der ACR mit Managed Identity unterstützen
az containerapp env create `
  --resource-group $ResourceGroup `
  --name $EnvironmentName `
  --location $Location `
  --environment-mode WorkloadProfiles | Out-Null

# Container App mit Platzhalter-Image anlegen
# --registry-identity system: Container App zieht Images aus der ACR mit ihrer System-Assigned-Identity
# (die Rolle AcrPull wird dabei automatisch zugewiesen, keine Registry-Passwörter nötig)
az containerapp create `
  --resource-group $ResourceGroup `
  --name $ContainerAppName `
  --environment $EnvironmentName `
  --image mcr.microsoft.com/dotnet/samples:aspnetapp-10.0 `
  --target-port 8080 `
  --ingress external `
  --min-replicas 0 `
  --max-replicas 1 `
  --registry-server $RegistryLoginServer `
  --registry-identity system | Out-Null

# Identität für GitHub Actions zum Aktualisieren der Container App anlegen
$GitHubRepoSubject = gh api "repos/$GitHubRepo" --jq '"\(.owner.login)@\(.owner.id)/\(.name)@\(.id)"'
if (-not $GitHubRepoSubject) { throw 'GitHub-Repository-IDs konnten nicht ermittelt werden.' }

$GitHubIdentity = az identity create `
  --resource-group $ResourceGroup `
  --name $GitHubIdentityName `
  --location $Location | ConvertFrom-Json

az identity federated-credential create `
  --resource-group $ResourceGroup `
  --identity-name $GitHubIdentityName `
  --name github-$($GitHubBranch -replace '[^a-zA-Z0-9-]', '-') `
  --issuer 'https://token.actions.githubusercontent.com' `
  --subject "repo:$($GitHubRepoSubject):ref:refs/heads/$GitHubBranch" `
  --audiences 'api://AzureADTokenExchange' | Out-Null

## Rolle zum Aktualisieren der Container App vergeben (= neues Image deployen)
$ResourceGroupId = az group show --name $ResourceGroup --query id --output tsv
az role assignment create `
  --assignee-object-id $GitHubIdentity.principalId `
  --assignee-principal-type ServicePrincipal `
  --role 'Container Apps Contributor' `
  --scope $ResourceGroupId | Out-Null

## Rolle zum Pushen von Images in die ACR vergeben
az role assignment create `
  --assignee-object-id $GitHubIdentity.principalId `
  --assignee-principal-type ServicePrincipal `
  --role AcrPush `
  --scope $Registry.id | Out-Null

# Variablen für GitHub Workflow definieren
$GitHubVariables = [ordered]@{
  AZURE_CLIENT_ID       = $GitHubIdentity.clientId
  AZURE_TENANT_ID       = $GitHubIdentity.tenantId
  AZURE_SUBSCRIPTION_ID = $SubscriptionId
  AZURE_RESOURCE_GROUP  = $ResourceGroup
  AZURE_CONTAINER_APP   = $ContainerAppName
  ACR_NAME              = $RegistryName
  ACR_LOGIN_SERVER      = $RegistryLoginServer
  IMAGE_NAME            = 'streamwriter'
}
foreach ($Entry in $GitHubVariables.GetEnumerator()) {
  gh variable set $Entry.Key --repo $GitHubRepo --body $Entry.Value
}
Write-Host "=== GitHub variables set in $GitHubRepo"

# Ersten Build und Deploy starten
gh workflow run deploy.yml --repo $GitHubRepo --ref $GitHubBranch
Write-Host "=== Deploy workflow started: https://github.com/$GitHubRepo/actions/workflows/deploy.yml"

$Fqdn = az containerapp show `
  --resource-group $ResourceGroup `
  --name $ContainerAppName `
  --query properties.configuration.ingress.fqdn `
  --output tsv
Write-Host "=== Container app running at https://$Fqdn"

<#
az group delete --name rg-streamwriter --yes --no-wait 
#>
