$ErrorActionPreference = "Stop"
$name = "minipdf-web"
$rg = "rg-minipdf-web"
$zipPath = "d:\git\MiniPdf\MiniPdf.Web\publish.zip"

Write-Host "Getting credentials..."
$creds = az webapp deployment list-publishing-credentials --name $name --resource-group $rg -o json 2>$null | ConvertFrom-Json
$user = $creds.publishingUserName
$pass = $creds.publishingPassword

Write-Host "Deploying $zipPath to $name..."
$url = "https://$name.scm.azurewebsites.net/api/zipdeploy"

curl.exe -X POST $url `
    -u "${user}:${pass}" `
    -H "Content-Type: application/zip" `
    --data-binary "@$zipPath" `
    -w "`nHTTP_STATUS:%{http_code}" `
    --max-time 300 `
    -s -o NUL

Write-Host "`nDone. Visit: https://$name.azurewebsites.net"
