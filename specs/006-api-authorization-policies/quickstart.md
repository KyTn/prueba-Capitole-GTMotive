# Quickstart: Verificación de autorización

## Prerequisites

- .NET SDK 9.0.203 or compatible latest patch.
- Docker Desktop only for the container verification.
- No real identity-provider credential is required for automated tests.

## Restore and build

```powershell
dotnet restore src/microservice.sln
dotnet build src/microservice.sln --configuration Release --no-restore
```

Expected: no compiler or analyzer errors.

## Automated test matrix

```powershell
dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj --configuration Release --no-build
dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests/GtMotive.Estimate.Microservice.FunctionalTests.csproj --configuration Release --no-build
dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/GtMotive.Estimate.Microservice.InfrastructureTests.csproj --configuration Release --no-build
```

Expected:

- Unit: catalog, attribute metadata, AND semantics, deduplication, short-circuit and invalid declarations pass.
- Functional: authorization calls receive the expected user/resource/policy and denied requests do not reach the application operation.
- Infrastructure: each of the four endpoints returns `401` without identity, `403` without its permission and its pre-existing result when authorized.

## Static coverage checks

```powershell
rg -n "\[AllowAnonymous\]" src/GtMotive.Estimate.Microservice.Api
rg -n "\[ApiAuthorization" src/GtMotive.Estimate.Microservice.Api
rg -n "JwtAuthority|UseAuthentication|UseAuthorization" src/GtMotive.Estimate.Microservice.Host
```

Expected:

- No `[AllowAnonymous]` remains on business controllers.
- Exactly the four business actions have catalog-backed authorization declarations.
- Host sources authentication from `JwtAuthority`, and authentication precedes authorization.

## Local smoke test

Set a non-secret development authority through configuration and start Host:

```powershell
$env:AppSettings__JwtAuthority = "https://identity.mygtmotive.com"
dotnet run --project src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
```

Invoke each business endpoint without a bearer credential. Expected: `401`; no business mutation occurs. Authorized manual testing requires a non-production test token containing the exact cataloged `permission` value.

## Docker verification

```powershell
docker compose build
docker compose up --detach
docker compose ps
```

Supply `AppSettings__JwtAuthority` through environment configuration, never in the image or repository. Confirm the health endpoint remains available and the four business endpoints require authentication.

## Contract review

Compare runtime OpenAPI with:

- [authorization-catalog.md](contracts/authorization-catalog.md)
- [http-authorization.md](contracts/http-authorization.md)

The review fails if a protected operation lacks bearer security, `401`/`403`, a cataloged resource, or at least one cataloged policy.

