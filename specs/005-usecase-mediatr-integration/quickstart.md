# Quickstart: Validar T5

## Restore and build

```powershell
dotnet restore src/microservice.sln
dotnet build src/microservice.sln --no-restore
```

## Test suites

```powershell
dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj --no-build
dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests/GtMotive.Estimate.Microservice.FunctionalTests.csproj --no-build
dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/GtMotive.Estimate.Microservice.InfrastructureTests.csproj --no-build
```

Debe comprobarse conformidad de cuatro casos de uso/inputs/outputs, un handler por mensaje, ausencia de casos de uso en constructores de controllers, publicación única en tres mutaciones, no-publicación en rechazo, telemetría para cuatro flujos y compatibilidad de todas las pruebas HTTP.

## Local

```powershell
dotnet run --project src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
```

Probar en Swagger `POST /vehicles`, `GET /vehicles`, `POST /rentals` y `POST /rentals/returns`. En Development, `NoOpTelemetry` permite ejecutar sin Application Insights.

## Docker

```powershell
docker compose up --build
docker compose ps
docker compose down
```

## Static checks

```powershell
rg -n "Controller\\([^)]*UseCase" src/GtMotive.Estimate.Microservice.Api
rg -n "IRequest|IRequestHandler" src/GtMotive.Estimate.Microservice.Api
```

Ambos deben devolver cero coincidencias; MediatR solo aparece en Api como `IMediator` de los controllers.
