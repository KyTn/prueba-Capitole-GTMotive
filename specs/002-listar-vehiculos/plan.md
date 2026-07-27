# Implementation Plan: Listar vehículos de la flota

**Branch**: `002-listar-vehiculos` | **Date**: 2026-07-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/002-listar-vehiculos/spec.md`

## Summary

Implementar el caso de uso de solo lectura `ListVehicles` sobre la arquitectura hexagonal existente. `ApplicationCore` solicitará todos los vehículos mediante una ampliación del puerto `IVehicleRepository`, materializará el resultado completo y lo proyectará al contrato de salida compartido. `Infrastructure` incorporará la lectura de la colección MongoDB; `Api` expondrá `GET /vehicles`; `Host` compondrá el caso de uso con los adaptadores existentes. La entrega tendrá una prueba unitaria aislada, una prueba funcional sin Host y una prueba de infraestructura que atraviese HTTP/Host.

## Technical Context

**Language/Version**: C# / .NET 9 (`global.json`: SDK 9.0.203)  
**Primary Dependencies**: ASP.NET Core, Microsoft.Extensions.DependencyInjection, MongoDB.Driver 2.19.0  
**Storage**: MongoDB, colección `vehicles`, accedida mediante `IVehicleRepository`  
**Testing**: xUnit 2.9.2, FluentAssertions 7.0.0, Microsoft.AspNetCore.Mvc.Testing; dobles manuales en memoria para pruebas aisladas y funcionales  
**Target Platform**: Servicio HTTP Linux, ejecución local con .NET 9 y contenedorizada con imágenes oficiales .NET 9  
**Project Type**: Microservicio web con proyectos Domain, ApplicationCore, Infrastructure, Api y Host  
**Performance Goals**: Listado completo visible en menos de 2 segundos para al menos el 95 % de consultas bajo carga operativa normal  
**Constraints**: Operación de solo lectura; colección vacía como resultado válido; sin filtros, ordenación contractual ni paginación; materialización completa antes de responder; `CancellationToken` propagado en límites asíncronos  
**Scale/Scope**: Un endpoint GET y un caso de uso; reutiliza una entidad, una colección, un puerto de persistencia y los tres proyectos de prueba existentes

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Dependency direction — PASS**: No se añaden reglas ni dependencias a Domain. `ListVehiclesUseCase` y el puerto de lectura viven en ApplicationCore; MongoDB implementa el puerto en Infrastructure; Api adapta HTTP y Host conserva la composición.
- **Domain invariants — PASS**: El listado no cambia estado ni introduce invariantes nuevas. Sólo devuelve entidades `Vehicle` ya válidas; no recalcula antigüedad ni altera disponibilidad, por lo que ningún adaptador puede crear un atajo de escritura.
- **Use cases and contracts — PASS**: `ListVehicles` es una acción independiente. El contrato [openapi.yaml](contracts/openapi.yaml) documenta `200` para colecciones con elementos o vacías y `500` para fallos inesperados. El puerto, caso de uso, repositorio y controlador propagan `CancellationToken`.
- **Test matrix — PASS**: Unit aísla el método de listado con un stub sin infraestructura; Functional integra el caso de uso con el repositorio en memoria sin Host; Infrastructure recorre `GET /vehicles` mediante Host. Ninguna prueba cuenta en dos categorías.
- **Reproducibility — PASS**: Se reutilizan el SDK fijado, `compose.yaml`, MongoDB 8.2.6, las imágenes oficiales .NET 9 y los comandos ya documentados. No se añaden secretos ni servicios instalados manualmente.
- **Quality and simplicity — PASS**: Se amplía `IVehicleRepository` en lugar de crear un puerto redundante, se comparte el DTO público de vehículo entre creación y listado, no se agregan paquetes, se actualizan OpenAPI/Swagger y DI, y se exige restore, build, analizadores y las tres suites.

### Post-design re-check

El modelo de lectura no crea estados paralelos ni modifica la entidad. El contrato hace explícita la colección vacía, el repositorio materializa por completo o falla, y la matriz de pruebas conserva fronteras distintas. No quedan `NEEDS CLARIFICATION` ni violaciones constitucionales.

## Project Structure

### Documentation (this feature)

```text
specs/002-listar-vehiculos/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
├── checklists/
│   └── requirements.md
└── tasks.md                 # Creado posteriormente por /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── GtMotive.Estimate.Microservice.Domain/
│   └── Vehicles/                         # Vehicle y RegistrationNumber existentes
├── GtMotive.Estimate.Microservice.ApplicationCore/
│   └── Vehicles/
│       ├── IVehicleRepository.cs         # Añadir lectura completa
│       ├── VehicleDto.cs                 # Salida compartida de vehículo
│       └── List/                         # ListVehiclesUseCase y resultado
├── GtMotive.Estimate.Microservice.Infrastructure/
│   └── MongoDb/Vehicles/                 # Lectura y mapeo documento-entidad
├── GtMotive.Estimate.Microservice.Api/
│   └── Vehicles/
│       ├── Create/                       # POST existente
│       └── List/                         # GET, respuesta y presenter
└── GtMotive.Estimate.Microservice.Host/
    └── Program.cs                        # Composición existente

test/
├── unit/
│   └── GtMotive.Estimate.Microservice.UnitTests/Vehicles/
├── functional/
│   └── GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/
└── infrastructure/
    └── GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/
```

**Structure Decision**: Se conserva la solución hexagonal actual y se añaden piezas cohesionadas dentro de los proyectos existentes. El DTO de vehículo se eleva al espacio común `ApplicationCore.Vehicles` para reutilizar el mismo modelo de salida en POST y GET sin duplicar contratos.

## Design Sequence

1. Extraer el `VehicleDto` existente al espacio compartido y mantener el contrato de creación compatible.
2. Ampliar `IVehicleRepository` con una lectura asíncrona completa y actualizar los dobles manuales.
3. Implementar `ListVehiclesUseCase`, su resultado inmutable y la prueba unitaria aislada para colección poblada y vacía.
4. Añadir la implementación MongoDB, incluido el mapeo inverso de documento a entidad, materializando toda la consulta antes de devolverla.
5. Crear la prueba funcional del caso de uso con repositorio en memoria y sin referencia a Host.
6. Añadir `GET /vehicles`, presentación, documentación Swagger/OpenAPI y registro del caso de uso.
7. Añadir la prueba de infraestructura mediante `VehicleApiFactory`, verificando flota poblada y vacía a nivel HTTP/Host.
8. Ejecutar restore, build, analizadores, las tres suites y las comprobaciones local/Compose del quickstart.

## Complexity Tracking

No hay violaciones constitucionales que requieran justificación.
