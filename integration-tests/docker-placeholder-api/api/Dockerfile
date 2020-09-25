FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build-env
COPY ./src /app/src
COPY ./test /app/test
COPY ./drug-placeholder-api.sln /app/app.sln
WORKDIR /app
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./integration-tests/docker-placeholder-api/api/build/appsettings.inttest.json .
COPY ./integration-tests/docker-placeholder-api/api/build/hosting.json .
## This does not need to wait for the loader or other resources.
## Any integration tests will need to wait for the API to report being healthy
EXPOSE 5001
ENV ASPNETCORE_ENVIRONMENT=inttest
ENV ASPNETCORE_URLS="http://*:5000"
ENTRYPOINT ["dotnet", "NCI.OCPL.Api.MyCoolProject.dll"]