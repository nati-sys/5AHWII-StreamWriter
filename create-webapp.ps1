az account show 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
  az login
}

$AppServicePlanName = 'P0v4'
Write-Host "Finding locations that support App Service Plan '$AppServicePlanName'."
$LocationCandidates = @(az policy assignment list --query "[?name == 'sys.regionrestriction'].parameters.listOfAllowedLocations.value[]" | ConvertFrom-Json)
if ($LocationCandidates.Count -eq 0) {
  $LocationCandidates = @(az account list-locations --query "[?metadata.geographyGroup == 'Europe'].name" | ConvertFrom-Json)
}
$SubscriptionId = az account show --query id -o tsv
$Locations = @()
foreach ($LocationCandidate in $LocationCandidates) {
  Write-Host -NoNewline "Checking $LocationCandidate ... "
  $Scope = "/subscriptions/$SubscriptionId/providers/Microsoft.Web/locations/$LocationCandidate"
  $Quota = az quota list --scope $Scope | ConvertFrom-Json
  $TotalLimit = ($Quota | Where-Object name -eq '*').properties.limit.value
  $PlanLimit = ($Quota | Where-Object name -eq $AppServicePlanName).properties.limit.value
  $Usage = az quota usage list --scope $Scope | ConvertFrom-Json
  $TotalUsage = ($Usage | Where-Object name -eq '*').properties.usages.value
  $PlanUsage = ($Usage | Where-Object name -eq $AppServicePlanName).properties.usages.value
  if ($TotalLimit -gt 0 -and $TotalLimit -gt $TotalUsage -and $PlanLimit -gt 0 -and $PlanLimit -gt $PlanUsage) {
    Write-Host 'free quota 🎉'
    $Locations += $LocationCandidate
  }
  else {
    Write-Host 'no quota left'
  }
}
if ($Locations.Count -eq 0) { throw 'No location found. You''re doomed.' }
else {
  Write-Host "Found possible locations: $Locations"
  $Location = $Locations[0]
  Write-Host "Using $Location"
}

$UserId = $(az account show --query "id" --output tsv).Substring(0, 8)

az group create --name rg-streamwriter --location $Location | Out-Null

az provider register --namespace Microsoft.Web --wait

az appservice plan create `
  --resource-group rg-streamwriter `
  --name asp-streamwriter `
  --location $Location `
  --sku P0v4 `
  --is-linux | Out-Null

az webapp create `
  --resource-group rg-streamwriter `
  --plan asp-streamwriter `
  --name wa-streamwriter-$UserId `
  --runtime "DOTNETCORE:10.0" `
  --https-only `
  --public-network-access Enabled `
  --assign-identity | Out-Null

$WebApp = az webapp show `
  --name wa-streamwriter-$UserId `
  --resource-group rg-streamwriter | ConvertFrom-Json
Write-Host "=== Web app running at https://$($WebApp.defaultHostName)"

<#
az group delete --name rg-streamwriter --yes --no-wait 
#>
