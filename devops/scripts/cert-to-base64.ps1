param([string]$CertPath)
$fullPath = Resolve-Path $CertPath
$base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($fullPath))
Write-Output "$base64"
