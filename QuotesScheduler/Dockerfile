FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app/QuotesScheduler

COPY ./QuotesLib ../QuotesLib

COPY ./QuotesScheduler/*.csproj .
RUN dotnet restore

COPY ./QuotesScheduler .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/QuotesScheduler/out .
ENTRYPOINT ["dotnet", "QuotesScheduler.dll"]
