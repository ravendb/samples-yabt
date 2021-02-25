#!/bin/bash -e

if [ -z "$Database__RavenDbUrls__0" ]; then
    echo "Database__RavenDbUrls__0 env var is required."
    exit 1
fi

if [ -z "$Database__Certificate" ]; then
    echo "Database__Certificate env var is required."
    exit 2
fi

exec dotnet WebApi.dll