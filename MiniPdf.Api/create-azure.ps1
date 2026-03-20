#!/usr/bin/env pwsh
# create-azure.ps1
# Provisions Azure resources for MiniPdf API and outputs the publish profile.
#
# Usage: .\create-azure.ps1
# Prerequisites: az CLI logged in (az login)
#
# After running, copy the publish profile output and add it as a GitHub secret:
#   Repository > Settings > Secrets and variables > Actions
#   Secret name: AZURE_WEBAPP_PUBLISH_PROFILE_API

$ErrorActionPreference = "Stop"

$rg       = "rg-minipdf-api"
$plan     = "asp-minipdf-api"
$appName  = "minipdf-api"
$location = "eastus"
$runtime  = "DOTNETCORE:9.0"
$sku      = "F1"

Write-Host "==> Creating resource group '$rg' in '$location'..."
az group create --name $rg --location $location --output none

Write-Host "==> Creating App Service plan '$plan' (Free F1, Linux)..."
az appservice plan create `
    --name $plan `
    --resource-group $rg `
    --sku $sku `
    --is-linux `
    --output none

Write-Host "==> Creating Web App '$appName' (runtime $runtime)..."
az webapp create `
    --name $appName `
    --resource-group $rg `
    --plan $plan `
    --runtime $runtime `
    --output none

Write-Host ""
Write-Host "==> Azure resources created successfully."
Write-Host "    App URL: https://$appName.azurewebsites.net"
Write-Host ""
Write-Host "==> Fetching publish profile (paste into GitHub Secret: AZURE_WEBAPP_PUBLISH_PROFILE_API)..."
Write-Host "---------- BEGIN PUBLISH PROFILE ----------"
az webapp deployment list-publishing-profiles `
    --name $appName `
    --resource-group $rg `
    --xml
Write-Host "----------  END PUBLISH PROFILE  ----------"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Copy the XML above."
Write-Host "  2. Go to https://github.com/mini-software/MiniPdf/settings/secrets/actions"
Write-Host "  3. Create secret named: AZURE_WEBAPP_PUBLISH_PROFILE_API"
Write-Host "  4. Push to 'main' branch - GitHub Actions will deploy automatically."
