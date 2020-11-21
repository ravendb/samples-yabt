FROM mcr.microsoft.com/dotnet/core/sdk:5.0 as build

WORKDIR /build
COPY . /build
RUN dotnet restore \
    && mkdir -p /publish \
    && dotnet publish -c Release -o /publish back-end/WebApi/WebApi.csproj \
    && cp devops/run.sh /publish/run.sh

FROM mcr.microsoft.com/dotnet/core/aspnet:5.0 AS runtime

COPY --from=build /publish /app
WORKDIR /app
ENV APP_SETTINGS='{}' 

CMD bash ./run.sh