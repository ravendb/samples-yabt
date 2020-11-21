$ErrorActionPreference = 'Stop'

docker build -t yabt -f $PSScriptRoot\..\back-end\WebApi\Dockerfile "$PSScriptRoot\.."

if ($LASTEXITCODE -ne 0) {
    throw "Failed to build Docker image."
}