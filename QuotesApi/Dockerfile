FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
ARG BUILD_VERSION=0.0.0.0
RUN dotnet publish -c Release -o out -p:AssemblyVersion=${BUILD_VERSION}

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "QuotesApi.dll"]
