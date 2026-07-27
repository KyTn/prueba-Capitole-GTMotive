# prueba-Capitole-GTMotive

Microservicio .NET 9 para gestionar vehículos y sus alquileres.

## Arquitectura

- `Domain` contiene el modelo, invariantes, eventos y puertos.
- `ApplicationCore` contiene comandos/queries MediatR, handlers y casos de uso conformes
  a `IUseCase`, `IUseCaseInput` e `IUseCaseOutput`.
- `Api` contiene DTOs HTTP puros, controllers y presenters. Los controllers mapean cada
  request HTTP al comando/query de ApplicationCore y lo envían mediante `IMediator`.
- `Infrastructure` implementa MongoDB, bus, telemetría, logging, tiempo y consulta de
  personas; `Host` compone la aplicación.

## Devolver un vehículo

Un vehículo con alquiler activo puede devolverse indicando la misma persona y vehículo:

```http
POST /rentals/returns
Content-Type: application/json

{
  "personId": "11111111-1111-1111-1111-111111111111",
  "vehicleId": "22222222-2222-2222-2222-222222222222"
}
```

La respuesta satisfactoria es `200 OK` e incluye el alquiler con estado `closed` y
`endedAt`. Una persona o vehículo inexistente devuelve `404`; identificadores inválidos
devuelven `400`; un vehículo sin alquiler activo, ya devuelto o perteneciente a otra
persona devuelve `409`.

La devolución cierra el alquiler de forma atómica, dejando disponibles tanto a la persona
como al vehículo para un alquiler posterior.

## Verificación

```powershell
dotnet build src/microservice.sln --configuration Release
dotnet test src/microservice.sln --configuration Release --no-build
docker compose config
```

## Ejecución fuera del contenedor

El Host local escucha en `http://localhost:8080`. MongoDB y MockServer pueden ejecutarse
como dependencias aisladas:

```powershell
docker compose up -d mongodb mockserver
dotnet run --project src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
Invoke-WebRequest http://localhost:8080/health/live
```

Las variables locales relevantes son:

- `MongoDb__ConnectionString` (por defecto `mongodb://localhost:27018`).
- `MongoDb__MongoDbDatabaseName`.
- `MongoDb__VehiclesCollectionName` y `MongoDb__RentalsCollectionName`.
- `PersonRegistry__BaseUrl` (por defecto `http://localhost:1080`).
- `ASPNETCORE_URLS` o el puerto `8080` definido en `launchSettings.json`.

## Ejecución en contenedores

Copiar `.env.example` a `.env` permite cambiar puertos, nombres de colecciones, nivel de
log y nombre del volumen sin editar Compose:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
docker compose ps
Invoke-WebRequest http://localhost:8080/health/live
Invoke-WebRequest http://localhost:8080/vehicles
docker compose down
```

- La aplicación publica `${APP_PORT:-8080}` y escucha internamente en `8080`.
- MockServer publica `${MOCKSERVER_PORT:-1080}` y carga
  `docker/mockserver/expectations.json` mediante un volumen de solo lectura.
- MongoDB usa el volumen persistente `${MONGODB_VOLUME_NAME}` en `/data/db` y publica
  `${MONGODB_PORT:-27018}` para que el Host local use `mongodb://localhost:27018`.
- La aplicación espera a MongoDB saludable y expone `/health/live`; Compose y la imagen
  usan esa ruta para comprobar el proceso.
- `docker compose down` conserva los datos. Para eliminar también el volumen:
  `docker compose down --volumes`.

Consulta la guía actual en
[`specs/005-usecase-mediatr-integration/quickstart.md`](specs/005-usecase-mediatr-integration/quickstart.md).
