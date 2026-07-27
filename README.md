# Prueba técnica Capitole · GT Motive

> **Aviso de propiedad intelectual**
>
> Este repositorio se ha creado exclusivamente como prueba técnica para Capitole.
> Salvo los componentes de terceros y los derechos que pudieran haberse cedido
> expresamente por contrato, el código y la documentación originales contenidos en
> él son propiedad de su autor. No se autoriza su copia, reproducción, modificación,
> distribución, publicación ni explotación, total o parcial, sin consentimiento
> previo y por escrito del titular de los derechos. El titular se reserva el
> ejercicio de las acciones legales que correspondan frente a cualquier uso no
> autorizado.

Microservicio REST desarrollado con .NET 9 para gestionar una flota de vehículos y
sus alquileres. La API permite crear y consultar vehículos, alquilarlos y registrar
su devolución. Los endpoints de negocio están protegidos mediante JWT y permisos.

## Tecnologías

- .NET SDK 9.0.203
- ASP.NET Core, MediatR y xUnit
- MongoDB 8
- IdentityServer/JWT para autenticación
- Docker Compose y MockServer para el registro de personas

## Arquitectura

La solución sigue una arquitectura hexagonal:

- `Domain`: entidades, invariantes, eventos y puertos de dominio.
- `ApplicationCore`: casos de uso, comandos y queries de MediatR.
- `Api`: controllers, contratos HTTP, presenters y autorización.
- `Infrastructure`: persistencia MongoDB, mensajería, telemetría, logging y acceso al
  registro de personas.
- `Host`: composición, configuración, autenticación y pipeline HTTP.

Los tests se dividen en `unit`, `functional` e `infrastructure`.

## Requisitos

- [.NET SDK 9.0.203](global.json), o un parche posterior compatible.
- Docker Desktop con Docker Compose, si se ejecutan las dependencias o toda la
  aplicación en contenedores.

## Configuración

En desarrollo, la configuración se encuentra en
[`appsettings.Development.json`](src/GtMotive.Estimate.Microservice.Host/appsettings.Development.json).
Los valores pueden sobrescribirse con variables de entorno usando `__` como
separador:

| Variable | Descripción | Valor local |
|---|---|---|
| `AppSettings__JwtAuthority` | Autoridad emisora de los JWT; es obligatoria | `https://identity.mygtmotive.com` |
| `MongoDb__ConnectionString` | Conexión a MongoDB | `mongodb://localhost:27018` |
| `MongoDb__MongoDbDatabaseName` | Base de datos | `prueba-capitole-gtmotive` |
| `MongoDb__VehiclesCollectionName` | Colección de vehículos | `vehicles` |
| `MongoDb__RentalsCollectionName` | Colección de alquileres | `rentals` |
| `PersonRegistry__BaseUrl` | URL del registro de personas | `http://localhost:1080` |
| `ASPNETCORE_URLS` | Direcciones en las que escucha el Host | `http://localhost:8080` |

No se deben almacenar tokens, credenciales ni valores de producción en el
repositorio.

## Ejecución local

Inicia MongoDB y MockServer, y después ejecuta el Host:

```powershell
docker compose up -d mongodb mockserver
dotnet run --project src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
```

Comprueba que el servicio está disponible:

```powershell
Invoke-WebRequest http://localhost:8080/health/live
```

La documentación OpenAPI/Swagger está disponible en el Host durante la ejecución.
La ruta de salud es pública; todos los endpoints de negocio requieren un bearer
token válido.

## Ejecución con Docker Compose

Puedes personalizar puertos, colecciones y opciones de Swagger copiando el archivo
de ejemplo:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
docker compose ps
Invoke-WebRequest http://localhost:8080/health/live
```

La aplicación se publica en `${APP_PORT:-8080}`, MockServer en
`${MOCKSERVER_PORT:-1080}` y MongoDB en `${MONGODB_PORT:-27018}`. El volumen de
MongoDB persiste al ejecutar `docker compose down`.

Para detener el entorno:

```powershell
docker compose down
```

## Autenticación y autorización

La API valida el JWT contra `AppSettings:JwtAuthority`. Cada operación exige un
claim `permission` exacto y sensible a mayúsculas:

| Método y ruta | Recurso | Permiso requerido |
|---|---|---|
| `POST /vehicles` | `Vehicles` | `Vehicles.Create` |
| `GET /vehicles` | `Vehicles` | `Vehicles.Read` |
| `POST /rentals` | `Rentals` | `Rentals.Create` |
| `POST /rentals/returns` | `Rentals` | `Rentals.Return` |

Una petición sin credenciales válidas devuelve `401 Unauthorized`; una identidad
autenticada sin el permiso requerido devuelve `403 Forbidden`. En ambos casos, el
controller y el caso de uso no se ejecutan.

Ejemplo de consulta autorizada:

```powershell
$token = "<JWT_DE_PRUEBA>"
Invoke-RestMethod `
  -Uri http://localhost:8080/vehicles `
  -Headers @{ Authorization = "Bearer $token" }
```

## API

### Crear un vehículo

```http
POST /vehicles
Authorization: Bearer <token con Vehicles.Create>
Content-Type: application/json

{
  "registrationNumber": "1234ABC",
  "brand": "Toyota",
  "model": "Corolla",
  "manufactureDate": "2022-01-15"
}
```

### Listar vehículos

```http
GET /vehicles
Authorization: Bearer <token con Vehicles.Read>
```

### Alquilar un vehículo

```http
POST /rentals
Authorization: Bearer <token con Rentals.Create>
Content-Type: application/json

{
  "personId": "11111111-1111-1111-1111-111111111111",
  "vehicleId": "22222222-2222-2222-2222-222222222222"
}
```

### Devolver un vehículo

```http
POST /rentals/returns
Authorization: Bearer <token con Rentals.Return>
Content-Type: application/json

{
  "personId": "11111111-1111-1111-1111-111111111111",
  "vehicleId": "22222222-2222-2222-2222-222222222222"
}
```

Una devolución correcta responde `200 OK` y cierra el alquiler de forma atómica.
Los errores de validación, recursos inexistentes y conflictos de negocio se
traducen respectivamente a respuestas `400`, `404` y `409`, según la operación.

## Compilación y pruebas

```powershell
dotnet restore src/microservice.sln
dotnet build src/microservice.sln --configuration Release --no-restore
dotnet test src/microservice.sln --configuration Release --no-build
docker compose config
```

La guía detallada de verificación y los contratos de autorización están en
[`specs/006-api-authorization-policies/quickstart.md`](specs/006-api-authorization-policies/quickstart.md).
