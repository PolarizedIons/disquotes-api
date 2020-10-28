FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app/QuotesBot

COPY ./QuotesLib ../QuotesLib

COPY ./QuotesBot/*.csproj .
COPY ./NuGet.Config ../
RUN dotnet restore

COPY ./QuotesBot .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/QuotesBot/out .
ENTRYPOINT ["dotnet", "QuotesBot.dll"]
