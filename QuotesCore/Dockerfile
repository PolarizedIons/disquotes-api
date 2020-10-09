FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app/QuotesCore

COPY ./QuotesLib ../QuotesLib

COPY ./QuotesCore/*.csproj .
RUN dotnet restore

COPY ./QuotesCore .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/QuotesCore/out .
ENTRYPOINT ["dotnet", "QuotesCore.dll"]
