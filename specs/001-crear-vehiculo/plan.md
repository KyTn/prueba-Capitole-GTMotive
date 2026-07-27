# Implementation Plan: Crear vehículo en la flota

**Branch**: `001-crear-vehiculo` | **Date**: 2026-07-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/001-crear-vehiculo/spec.md`

## Summary

Implementar el caso de uso `CreateVehicle` siguiendo la arquitectura hexagonal existente. El dominio construirá un `Vehicle` válido a partir de matrícula, marca, modelo, fecha de fabricación y una fecha de alta suministrada por una abstracción temporal. ApplicationCore coordinará la comprobación de unicidad y el guardado mediante un puerto; Infrastructure aportará el adaptador MongoDB con índice único; Api traducirá `POST /vehicles` y sus errores; Host compondrá las dependencias. La entrega incluirá pruebas unitarias puras, funcionales sin Host e infraestructura atravesando HTTP/Host.

## Technical Context

**Language/Version**: C# / .NET 9 (`global.json`: SDK 9.0.203)  
**Primary Dependencies**: ASP.NET Core, MediatR 10.0.1, MongoDB.Driver 2.19.0, Microsoft.Extensions.DependencyInjection  
**Storage**: MongoDB, colección `vehicles`, accedida únicamente mediante un puerto de ApplicationCore  
**Testing**: xUnit 2.9.2, FluentAssertions 7.0.0, Microsoft.AspNetCore.TestHost; dobles en memoria para unitarias/funcionales y MongoDB de Docker para infraestructura  
**Target Platform**: Servicio HTTP Linux, ejecución local con .NET 9 y contenedorizada con imágenes oficiales .NET 9  
**Project Type**: Microservicio web con proyectos Domain, ApplicationCore, Infrastructure, Api y Host  
**Performance Goals**: Confirmación del alta en menos de 2 segundos para al menos el 95 % de solicitudes bajo carga operativa normal  
**Constraints**: Antigüedad máxima de cinco años naturales; exactamente cinco años es válido; fecha futura inválida; matrícula única normalizada; alta atómica; `CancellationToken` propagado en límites asíncronos; sin secretos ni servicios instalados manualmente  
**Scale/Scope**: Un endpoint y un caso de uso; una entidad y una colección; cuatro resultados de negocio; tres proyectos de prueba separados

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Dependency direction — PASS**: `Vehicle`, `RegistrationNumber` y reglas temporales viven en Domain sin dependencias externas. ApplicationCore define `ICreateVehicleUseCase`, `IVehicleRepository` e `IClock`. Infrastructure implementa persistencia; Api es adaptador de entrada; Host compone.
- **Domain invariants — PASS**: `Vehicle.Create` protege fecha futura y antigüedad; `RegistrationNumber` normaliza y valida. La unicidad se expresa como puerto y se garantiza definitivamente con índice único en el adaptador, traduciendo la colisión a un resultado de dominio.
- **Use cases and contracts — PASS**: Existe una acción `CreateVehicle`; los puertos no conocen HTTP ni MongoDB. El contrato [openapi.yaml](contracts/openapi.yaml) mapea `201`, `400`, `409`, `422` y `500`. Todas las operaciones asíncronas reciben y propagan `CancellationToken`.
- **Test matrix — PASS**: Unit prueba la fábrica/regla sin dependencias; Functional integra caso de uso con repositorio y reloj controlados sin Host; Infrastructure usa Host HTTP y MongoDB aislado. Ninguna prueba cuenta en más de una categoría.
- **Reproducibility — PASS (plan obligation)**: La implementación debe verificar `dotnet` local y Compose, fijar una versión compatible de MongoDB en vez de `latest`, y resolver la referencia actualmente inexistente a `docker-compose.dcproj` o añadir el proyecto requerido. No se incorporan secretos.
- **Quality and simplicity — PASS**: Se reutilizan proyectos y paquetes existentes. Las únicas abstracciones nuevas (`IVehicleRepository`, `IClock`) protegen dependencias reales. Se exigen restore, build, analizadores, pruebas, OpenAPI actualizado y logs estructurados sin datos sensibles.

### Post-design re-check

El modelo, contrato y quickstart mantienen la dirección de dependencias, hacen explícitas las fronteras de consistencia y trazan las tres categorías de prueba. No quedan `NEEDS CLARIFICATION` ni excepciones constitucionales.

## Project Structure

### Documentation (this feature)

```text
specs/001-crear-vehiculo/
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
│   └── Vehicles/            # Vehicle, RegistrationNumber, errores
├── GtMotive.Estimate.Microservice.ApplicationCore/
│   └── Vehicles/Create/     # Caso de uso, modelos de entrada/salida y puertos
├── GtMotive.Estimate.Microservice.Infrastructure/
│   └── MongoDb/Vehicles/    # Documento, repositorio e índice único
├── GtMotive.Estimate.Microservice.Api/
│   └── Vehicles/Create/     # Controller, request, response y presenter/mapeo
└── GtMotive.Estimate.Microservice.Host/
    └── Program.cs           # Composición

test/
├── unit/
│   └── GtMotive.Estimate.Microservice.UnitTests/
├── functional/
│   └── GtMotive.Estimate.Microservice.FunctionalTests/
└── infrastructure/
    └── GtMotive.Estimate.Microservice.InfrastructureTests/
```

**Structure Decision**: Se conserva la solución hexagonal existente. Se crean tres proyectos de prueba independientes en los directorios ya presentes porque actualmente no contienen fuentes versionadas y la constitución exige separación inequívoca.

## Design Sequence

1. Crear los tipos de dominio y sus pruebas puras, incluida la frontera exacta de cinco años y el 29 de febrero.
2. Definir el puerto de persistencia, reloj y contrato del caso de uso en ApplicationCore.
3. Implementar el caso de uso y las pruebas funcionales con adaptadores controlados, sin referenciar Host.
4. Implementar el documento/repositorio MongoDB y crear un índice único sobre la matrícula normalizada.
5. Añadir el adaptador HTTP, el mapeo de errores, OpenAPI y el registro de dependencias.
6. Añadir pruebas de infraestructura a nivel Host con base de datos aislada.
7. Corregir y verificar la ejecución local/Compose y ejecutar todas las puertas de calidad.

## Complexity Tracking

No hay violaciones constitucionales que requieran justificación.
