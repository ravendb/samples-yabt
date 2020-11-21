param([string]$AppSettingsPath)

$outputEnvFile = "setup.env"

[IO.File]::WriteAllText($outputEnvFile, "")

$appSettingsNoEol = [IO.File]::ReadAllText($AppSettingsPath).Replace("`r", "").Replace("`n", "")
[IO.File]::AppendAllText($outputEnvFile, "APP_SETTINGS=$appSettingsNoEol")
