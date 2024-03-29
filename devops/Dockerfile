# See the list of tags at https://mcr.microsoft.com/v2/dotnet/sdk/tags/list
FROM mcr.microsoft.com/dotnet/sdk:8.0.100 as back-end-build

WORKDIR /build
COPY . /build
RUN dotnet restore back-end/WebApi/WebApi.csproj \
    && mkdir -p /publish \
    && dotnet publish -c Release -o /publish back-end/WebApi/WebApi.csproj \
    && cp devops/run.sh /publish/run.sh
    
FROM node:lts-alpine3.18 as front-end-build 

WORKDIR /client
COPY ./front-end /client
RUN npm install --legacy-peer-deps \
    && node_modules/.bin/ng build --configuration production

# See the list of tags at https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list
FROM mcr.microsoft.com/dotnet/aspnet:8.0.0 AS runtime

COPY --from=back-end-build /publish /app
COPY --from=front-end-build /client/dist /app/wwwroot
WORKDIR /app
ENV Database__RavenDbUrls__0='' Database__Certificate='' 

CMD bash ./run.sh
