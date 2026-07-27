# Quickstart: Alquilar un vehículo

## Prerequisites

- .NET SDK 9.0.203 (selected by `global.json`)
- Docker Desktop or compatible Docker Engine for the container path
- No manually installed MongoDB is required

## Restore and build

```powershell
dotnet restore src/microservice.sln
dotnet build src/microservice.sln --configuration Release --no-restore
```

## Run the three test boundaries

```powershell
dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj --configuration Release
dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests/GtMotive.Estimate.Microservice.FunctionalTests.csproj --configuration Release
dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/GtMotive.Estimate.Microservice.InfrastructureTests.csproj --configuration Release
```

Expected coverage:

- Unit: `Rental.Create` and invariant-facing results without external dependencies.
- Functional: valid rental, missing references and conflicts using in-memory ports without Host.
- Infrastructure: `POST /rentals` through `WebApplicationFactory<Program>`, including `201` and `409`.

## Run locally

Start MongoDB through Compose, then Host:

```powershell
docker compose up -d mongodb mockserver
dotnet run --project src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
```

Seed or configure a known person in the environment's person registry and create a vehicle through `POST /vehicles`. Then rent it:

```http
POST /rentals
Content-Type: application/json

{
  "personId": "11111111-1111-1111-1111-111111111111",
  "vehicleId": "22222222-2222-2222-2222-222222222222"
}
```

The first valid request returns `201`. A second request for another vehicle with the same person, or for the same vehicle with another person, returns `409`. Unknown references return `404`; empty/malformed identifiers return `400`.

## Run with Docker Compose

```powershell
docker compose build
docker compose up -d
docker compose ps
```

Use `http://localhost:8080` unless the active path-base configuration adds a prefix. MongoDB 8.2.6 and MockServer 5.15.0 are supplied by Compose; no secret is committed or passed in the rental request.

## Quality checks

```powershell
dotnet build src/microservice.sln --configuration Release --no-restore
dotnet test src/microservice.sln --configuration Release --no-build
docker compose config
```

Verify that OpenAPI exposes `POST /rentals`, that logs include outcome and rental/vehicle identifiers without personal data, and that concurrent tests leave at most one active rental per person and vehicle.
