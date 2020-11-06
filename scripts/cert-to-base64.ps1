param([string]$CertPath)

Write-Output [Convert]::ToBase64String([IO.File]::ReadAllBytes($CertPath))
