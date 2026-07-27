FROM mcr.microsoft.com/dotnet/sdk:9.0.203 AS build
WORKDIR /src

COPY . .

RUN dotnet restore \
    "src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj"

RUN dotnet publish \
    "src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=5s --start-period=15s --retries=5 \
    CMD curl --fail --silent http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "GtMotive.Estimate.Microservice.Host.dll"]
