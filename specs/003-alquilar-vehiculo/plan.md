# Implementation Plan: Alquilar un vehículo

**Branch**: `003-alquilar-vehiculo` | **Date**: 2026-07-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/003-alquilar-vehiculo/spec.md`

## Summary

Implementar el caso de uso `RentVehicle` sobre la arquitectura hexagonal existente. El dominio incorporará el agregado `Rental`, el estado `Active` y el valor `PersonId`; `ApplicationCore` comprobará persona y vehículo mediante puertos y solicitará una creación atómica del alquiler. La disponibilidad se derivará de la ausencia de un alquiler activo, y MongoDB protegerá concurrentemente la exclusividad por persona y vehículo mediante índices únicos parciales. `Api` expondrá `POST /rentals`; `Host` compondrá los adaptadores. La entrega incluirá pruebas unitarias aisladas, funcionales sin Host e infraestructura atravesando HTTP/Host.

## Technical Context

**Language/Version**: C# / .NET 9 (`global.json`: SDK 9.0.203)  
**Primary Dependencies**: ASP.NET Core, Microsoft.Extensions.DependencyInjection, MongoDB.Driver 2.19.0; sin paquetes nuevos  
**Storage**: MongoDB, colecciones existentes `vehicles` y nueva `rentals`; la existencia de persona se consulta mediante `IPersonRegistry`  
**Testing**: xUnit 2.9.2, Microsoft.AspNetCore.Mvc.Testing; dobles manuales thread-safe para Unit, Functional e Infrastructure  
**Target Platform**: Servicio HTTP Linux, ejecución local con .NET 9 y contenedorizada con imágenes oficiales .NET 9  
**Project Type**: Microservicio web con proyectos Domain, ApplicationCore, Infrastructure, Api y Host  
**Performance Goals**: Confirmación o rechazo visible en menos de 2 segundos para al menos el 95 % de solicitudes bajo carga operativa normal  
**Constraints**: Un alquiler activo por persona y por vehículo; consistencia ante concurrencia; escritura atómica; `CancellationToken` propagado; sin devolución, pagos ni duración en T3  
**Scale/Scope**: Un endpoint POST, un caso de uso, un agregado, una colección y un puerto de identidad; ampliación de los tres proyectos de prueba existentes

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Dependency direction — PASS**: `Rental`, `PersonId` y las invariantes viven en Domain sin dependencias técnicas. Los puertos y `RentVehicleUseCase` viven en ApplicationCore; MongoDB y el registro de personas son adaptadores secundarios; Api traduce HTTP y Host conserva la composición.
- **Domain invariants — PASS**: `Rental.Create` protege identificadores, estado inicial y fecha de inicio. La frontera de repositorio `TryAddActiveAsync` persiste un único agregado y aplica la exclusividad concurrente por `PersonId` y `VehicleId`; ningún controlador decide disponibilidad.
- **Use cases and contracts — PASS**: `RentVehicle` es una acción independiente. [openapi.yaml](contracts/openapi.yaml) documenta `201`, `400`, `404`, `409` y `500`. Todos los puertos asíncronos propagan `CancellationToken`.
- **Test matrix — PASS**: Unit valida el agregado sin dependencias; Functional integra caso de uso, puertos y adaptadores en memoria sin Host; Infrastructure recorre `POST /rentals` mediante Host. Concurrencia por persona y vehículo tiene cobertura explícita.
- **Reproducibility — PASS**: Se reutilizan .NET 9, Docker/Compose, MongoDB 8.2.6 y MockServer 5.15.0. No se añaden secretos ni servicios instalados manualmente.
- **Quality and simplicity — PASS**: Un documento `Rental` es la única fuente de disponibilidad y evita una transacción entre vehículo y alquiler. Se reutilizan logging, resultados, presentadores, DI y pruebas existentes; no se añaden paquetes.

### Post-design re-check

El contrato, el modelo y la persistencia comparten una única definición de alquiler activo. Los índices parciales protegen las dos exclusividades bajo carreras y permiten que una futura devolución libere persona y vehículo al cerrar el alquiler. La comprobación previa mejora los errores, pero la restricción definitiva permanece en la escritura atómica. No quedan `NEEDS CLARIFICATION` ni violaciones constitucionales.

## Project Structure

### Documentation (this feature)

```text
specs/003-alquilar-vehiculo/
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
│   └── Rentals/                          # Rental, RentalStatus y PersonId
├── GtMotive.Estimate.Microservice.ApplicationCore/
│   ├── People/                           # IPersonRegistry
│   ├── Vehicles/IVehicleRepository.cs    # Lectura por id
│   └── Rentals/
│       ├── IRentalRepository.cs
│       └── Rent/                         # Comando, resultado y caso de uso
├── GtMotive.Estimate.Microservice.Infrastructure/
│   ├── MongoDb/Vehicles/                 # Lectura por id
│   ├── MongoDb/Rentals/                  # Documento, mapper, índices y repositorio
│   └── People/                           # Adaptador del registro de personas
├── GtMotive.Estimate.Microservice.Api/
│   └── Rentals/Rent/                     # Request, response, presenter y controller
└── GtMotive.Estimate.Microservice.Host/
    └── Program.cs                        # Composición existente

test/
├── unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/
├── functional/GtMotive.Estimate.Microservice.FunctionalTests/
│   ├── Rentals/
│   └── TestDoubles/
└── infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/
```

**Structure Decision**: Se conserva la solución hexagonal y se añaden carpetas cohesionadas en los proyectos existentes. `Rental` es el agregado y documento de consistencia; `Vehicle` no duplica un indicador mutable de disponibilidad. `IPersonRegistry` expresa la dependencia ya asumida por la especificación sin introducir datos personales en el dominio de alquiler.

## Design Sequence

1. Añadir `PersonId`, `RentalStatus` y `Rental.Create`, con pruebas unitarias de identificadores, inicio y estado activo.
2. Definir `IPersonRegistry`, `IRentalRepository`, lectura de vehículo por identificador y contratos de comando/resultado.
3. Implementar `RentVehicleUseCase`: validar entrada, comprobar persona y vehículo, crear agregado e intentar persistirlo atómicamente.
4. Crear `RentalDocument`, mapper, colección e índices únicos parciales para persona y vehículo activos; traducir duplicados a conflictos tipados.
5. Añadir dobles concurrentes y pruebas funcionales de éxito, inexistencia, segundo vehículo por persona y vehículo ocupado.
6. Añadir `POST /rentals`, request/response/presenter, mapeo `400`/`404`/`409`, Swagger/OpenAPI y registros DI.
7. Ampliar la factory Host con dobles controlados y probar `201`, `404`, `409` y carreras observables por HTTP.
8. Ejecutar restore, build, analizadores, las tres suites y las verificaciones local/Compose de [quickstart.md](quickstart.md).

## Complexity Tracking

No hay violaciones constitucionales que requieran justificación.
