# Quickstart: Crear vehículo en la flota

## Prerequisites

- .NET SDK 9.0.203 or a compatible latest patch.
- Docker with Docker Compose for MongoDB and container verification.
- No local MongoDB installation is required.

## Local verification

Start the dependency:

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

Create a valid vehicle, using a manufacture date no older than exactly five years from the request date:

```powershell
$body = @{
    registrationNumber = '1234ABC'
    brand = 'Toyota'
    model = 'Corolla'
    manufactureDate = '2024-06-15'
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri 'http://localhost:5000/vehicles' -ContentType 'application/json' -Body $body
```

Use the actual URL printed by Host if its development profile selects a different port.

## Container verification

```powershell
docker compose build
docker compose up -d
Invoke-WebRequest -Uri 'http://localhost:8080/swagger/index.html'
docker compose ps
```

The Compose environment pins MongoDB 8.2.6 and MockServer 5.15.0. The stale
`DockerComposeProjectPath` reference has been removed from Host.

## Expected quality gates

```powershell
dotnet test src/microservice.sln --configuration Release --no-build
docker compose config
docker compose build
```

All unit, functional and infrastructure tests must pass. The infrastructure suite must verify `201` and at least one business error through Host, while the functional suite must not reference Host.

## Verified result

Validated on 2026-07-27:

- Release build completed with zero warnings and zero errors.
- 9 unit, 3 functional and 5 infrastructure tests passed.
- `docker compose config` and `docker compose build` completed successfully.
- Compose reported MongoDB healthy; Swagger returned `200` and `POST /vehicles`
  returned `201` with a `Location` header.
