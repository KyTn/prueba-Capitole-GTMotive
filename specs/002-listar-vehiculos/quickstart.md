# Quickstart: Listar vehículos de la flota

## Prerequisites

- .NET SDK 9.0.203 or a compatible latest patch.
- Docker with Docker Compose for MongoDB and container verification.
- No local MongoDB installation is required.

## Local verification

Start MongoDB:

```powershell
docker compose up -d mongodb
```

Restore and build:

```powershell
dotnet restore src/microservice.sln
dotnet build src/microservice.sln --configuration Release --no-restore
```

Run each test boundary independently:

```powershell
dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests --configuration Release
dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests --configuration Release
dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests --configuration Release
```

Run the Host:

```powershell
dotnet run --project src/GtMotive.Estimate.Microservice.Host
```

List the fleet:

```powershell
Invoke-RestMethod -Method Get -Uri 'http://localhost:5000/vehicles'
```

Use the actual URL printed by Host if its development profile selects a different port. Before creating vehicles, the expected JSON result is `[]`. After adding vehicles through `POST /vehicles`, every registered vehicle must appear exactly once.

## Container verification

```powershell
docker compose config
docker compose build
docker compose up -d
Invoke-RestMethod -Method Get -Uri 'http://localhost:8080/vehicles'
docker compose ps
```

The Compose environment pins MongoDB 8.2.6 and MockServer 5.15.0. No manually installed external service or committed secret is required.

## Expected quality gates

```powershell
dotnet test src/microservice.sln --configuration Release --no-build
docker compose config
docker compose build
```

All unit, functional and infrastructure tests must pass. The infrastructure suite must verify `GET /vehicles` through Host for a populated fleet and an empty fleet. The functional suite must not reference Host, and the unit suite must not use infrastructure.

## Verified result

Validated on 2026-07-27:

- Restore completed successfully from the configured package sources.
- Release build completed with zero warnings and zero errors.
- 12 unit, 6 functional and 7 infrastructure tests passed.
- `docker compose config` and `docker compose build` completed successfully.
- Compose reported MongoDB healthy and the Host listening on port `8080`.
- Containerized `GET /vehicles` returned the complete persisted collection.
- Runtime Swagger exposed both `GET /vehicles` and the existing `POST /vehicles`; GET documents `200` and `500`.
