$ErrorActionPreference = 'Stop'

docker build -t yabt -f $PSScriptRoot\..\Dockerfile "$PSScriptRoot\..\.."

if ($LASTEXITCODE -ne 0) {
    throw "Failed to build Docker image."
}