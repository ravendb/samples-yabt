
$cert = $env:YABT_RAVENDB_CERT
$url = $env:YABT_RAVENDB_URL

docker run -it --rm -p 8080:8080 `
    -e Database__RavenDbUrls__0='$url' `
    -e Database__Certificate='$cert' `
    -e ASPNETCORE_URLS=http://0.0.0.0:8080 `
    yabt 