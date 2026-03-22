#!/usr/bin/env pwsh
# install-fonts.ps1
# Configures Azure App Service to use startup.sh which auto-downloads CJK fonts.
#
# Usage:  .\install-fonts.ps1
# Prerequisites: az CLI logged in (az login)

$ErrorActionPreference = "Stop"

$rg      = "rg-minipdf-api"
$appName = "minipdf-api"

Write-Host "==> Setting startup command to startup.sh ..."
az webapp config set `
    --resource-group $rg `
    --name $appName `
    --startup-file "/home/site/wwwroot/startup.sh" `
    --output none

Write-Host "==> Uploading startup.sh via Kudu ..."
$profile = az webapp deployment list-publishing-profiles `
    --resource-group $rg `
    --name $appName `
    --query "[?publishMethod=='MSDeploy'].{user:userName, pass:userPWD}" `
    --output json | ConvertFrom-Json

$user = $profile.user
$pass = $profile.pass
$kuduUrl = "https://$appName.scm.azurewebsites.net/api/vfs/site/wwwroot/startup.sh"

$bytes = [System.IO.File]::ReadAllBytes("$PSScriptRoot\startup.sh")
$headers = @{
    Authorization = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${user}:${pass}"))
    "If-Match"    = "*"
}
Invoke-RestMethod -Uri $kuduUrl -Method Put -Headers $headers -Body $bytes -ContentType "application/octet-stream"

Write-Host ""
Write-Host "==> Done! Restarting app to trigger font download ..."
az webapp restart --resource-group $rg --name $appName --output none
Write-Host "==> App restarted. Fonts will be downloaded on first startup."
Write-Host "    Check logs: az webapp log tail --resource-group $rg --name $appName"
