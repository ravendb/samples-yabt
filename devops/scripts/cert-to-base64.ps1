param([string]$CertPath)

$base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($CertPath))
Write-Output "$base64"
