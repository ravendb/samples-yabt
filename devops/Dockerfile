# See the list of tags at https://mcr.microsoft.com/v2/dotnet/sdk/tags/list
FROM mcr.microsoft.com/dotnet/sdk:5.0.302 as back-end-build

WORKDIR /build
COPY . /build
RUN dotnet restore back-end/WebApi/WebApi.csproj \
    && mkdir -p /publish \
    && dotnet publish -c Release -o /publish back-end/WebApi/WebApi.csproj \
    && cp devops/run.sh /publish/run.sh
    
FROM mhart/alpine-node:14 as front-end-build 

WORKDIR /client
COPY ./front-end /client
RUN npm install \ 
    && node_modules/.bin/ng build --prod

# See the list of tags at https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list
FROM mcr.microsoft.com/dotnet/aspnet:5.0.8 AS runtime

COPY --from=back-end-build /publish /app
COPY --from=front-end-build /client/dist /app/wwwroot
WORKDIR /app
ENV Database__RavenDbUrls__0='' Database__Certificate='' 

CMD bash ./run.sh
