FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app/QuotesApi

COPY ./QuotesLib ../QuotesLib

COPY ./QuotesApi/*.csproj .
COPY ./NuGet.Config ../
RUN dotnet restore

COPY ./QuotesApi .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/QuotesApi/out .
ENTRYPOINT ["dotnet", "QuotesApi.dll"]
