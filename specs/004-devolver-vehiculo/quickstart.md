# Quickstart: Devolver un vehículo

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

- Unit: `Rental.Return`, including valid closure, repeated return and invalid temporal ordering without external dependencies.
- Functional: valid return, vehicle not rented, wrong person and concurrent returns using in-memory ports without Host.
- Infrastructure: `POST /rentals/returns` through `WebApplicationFactory<Program>`, including `200`, `400`, `404`, `409` and final observable state.

## Run locally

Start MongoDB and the person registry stub through Compose, then Host:

```powershell
docker compose up -d mongodb mockserver
dotnet run --project src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
```

Create a vehicle and an active rental using the existing endpoints. Return it with the same person and vehicle:

```http
POST /rentals/returns
Content-Type: application/json

{
  "personId": "11111111-1111-1111-1111-111111111111",
  "vehicleId": "22222222-2222-2222-2222-222222222222"
}
```

The first valid request returns `200` with `status: "closed"` and `endedAt`. Repeating it, returning a vehicle without an active rental, or using another person returns `409`. Unknown person/vehicle references return `404`; empty or malformed identifiers return `400`.

## Run with Docker Compose

```powershell
docker compose build
docker compose up -d
docker compose ps
```

Use `http://localhost:8080` unless the active path-base configuration adds a prefix. MongoDB 8.2.6 and MockServer 5.15.0 are supplied by Compose; no secret is committed or included in the return request.

## Quality checks

```powershell
dotnet build src/microservice.sln --configuration Release --no-restore
dotnet test src/microservice.sln --configuration Release --no-build
docker compose config
```

Verify that OpenAPI exposes `POST /rentals/returns`, logs include outcome and rental/vehicle identifiers without personal data, a returned vehicle can subsequently be rented again, and concurrent return tests record one immutable `EndedAt`.
