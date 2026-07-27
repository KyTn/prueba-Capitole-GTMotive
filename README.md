# prueba-Capitole-GTMotive

Microservicio .NET 9 para gestionar vehículos y sus alquileres.

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

Consulta la guía completa en
[`specs/004-devolver-vehiculo/quickstart.md`](specs/004-devolver-vehiculo/quickstart.md).
