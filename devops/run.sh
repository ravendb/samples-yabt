#!/bin/bash

if [ -z "$APP_SETTINGS" ]; then
    echo "APP_SETTINGS is required."
    exit 1
else
    echo "$APP_SETTINGS" > appsettings.docker.json
fi

dotnet WebApi.dll --environment=docker