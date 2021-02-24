FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /build
COPY . /build
RUN dotnet restore back-end/WebApi/WebApi.csproj \
    && mkdir -p /publish \
    && dotnet publish -c Release -o /publish back-end/WebApi/WebApi.csproj \
    && cp devops/run.sh /publish/run.sh

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime

COPY --from=build /publish /app
WORKDIR /app
ENV SETTINGS_RAVENDB_URL='' SETTINGS_RAVENDB_CERTIFICATE='' 
RUN apt-get update && apt-get install -y gettext

CMD bash ./run.sh
