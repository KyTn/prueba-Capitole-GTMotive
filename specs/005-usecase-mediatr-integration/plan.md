# Implementation Plan: Integración de casos de uso, MediatR, eventos y telemetría

**Branch**: `005-usecase-mediatr-integration` | **Date**: 2026-07-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/005-usecase-mediatr-integration/spec.md`

## Summary

Adaptar los cuatro flujos HTTP existentes —crear vehículo, listar vehículos, alquilar y devolver— para que sus controladores dependan solo de `IMediator`. Cada request HTTP se mapeará a un comando/query `IRequest<TResultado>` de ApplicationCore, donde residirá su handler MediatR y se invocará el caso de uso tipado. Los DTO de Api permanecerán libres de MediatR. Los comandos implementarán `IUseCaseInput`, los resultados `IUseCaseOutput` y cada caso de uso `IUseCase<TInput>`, conservando su método tipado y cancelable. Los handlers de las tres mutaciones publicarán un evento de dominio únicamente ante éxito mediante `IBusFactory`; los cuatro medirán duración y resultado con `ITelemetry`.

## Technical Context

**Language/Version**: C# / .NET 9 (`global.json`: SDK 9.0.203)  
**Primary Dependencies**: ASP.NET Core, MediatR 10.0.1, MediatR.Extensions.Microsoft.DependencyInjection 10.0.1, Microsoft.Extensions.DependencyInjection; sin paquetes nuevos  
**Storage**: MongoDB mediante los repositorios existentes; no cambian documentos, índices ni consultas  
**Testing**: xUnit 2.9.2, Microsoft.AspNetCore.Mvc.Testing 9.0.0 y dobles manuales de bus/telemetría  
**Target Platform**: Servicio HTTP Linux, local con .NET 9 y contenedorizado con imágenes oficiales .NET 9  
**Project Type**: Microservicio web con Domain, ApplicationCore, Infrastructure, Api y Host  
**Performance Goals**: Al menos el 95 % de solicitudes mantiene una respuesta visible en menos de 2 segundos bajo carga normal  
**Constraints**: HTTP compatible; requests HTTP libres de MediatR; mensajes y handlers MediatR en ApplicationCore; cancelación propagada; eventos solo tras éxito persistido y máximo una vez por ejecución; telemetría sin datos sensibles; fallo de bus propagado; implementación de telemetría desactivada no bloqueante  
**Scale/Scope**: Cuatro controladores, cuatro mensajes/handlers, cuatro casos de uso con sus entradas/salidas, tres eventos de mutación, DI y tres categorías de pruebas

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Dependency direction — PASS**: Domain solo recibe hechos de negocio y sigue sin frameworks. Los mensajes `IRequest<T>` y handlers viven junto a los casos de uso en ApplicationCore. Api conserva DTOs HTTP, controllers, `IMediator` y presentación; Infrastructure conserva adaptadores y Host la composición.
- **Domain invariants — PASS**: Los handlers no crean vehículos ni alteran alquileres; delegan en los casos de uso existentes. Solo publican después de que el resultado confirme el cambio persistido. El listado no publica evento.
- **Use cases and contracts — PASS**: Crear, listar, alquilar y devolver siguen independientes. Entradas/salidas adoptan los marcadores suministrados; controladores solo traducen, envían y devuelven. El token HTTP llega al handler y al `ExecuteAsync`.
- **Test matrix — PASS**: Unit valida contratos y handlers con dobles; Functional integra handler, caso de uso, bus y telemetría sin Host; Infrastructure recorre los cuatro endpoints con Host.
- **Reproducibility — PASS**: No se añaden servicios, paquetes, puertos ni secretos; se reutilizan .NET 9, Docker/Compose, MongoDB y MockServer.
- **Quality and simplicity — PASS**: Se reutilizan MediatR, `IBusFactory`, `ITelemetry`, comandos, resultados y presentadores. Los handlers son adaptadores delgados y la telemetría no contiene datos personales.

### Post-design re-check

El diseño mantiene mensajes, handlers y casos de uso en ApplicationCore; Api solo traduce transporte y presenta resultados. Los eventos viven en Domain como hechos inmutables y su transporte se inicia en el handler solo tras un resultado exitoso. Los contratos de compatibilidad referencian los cuatro OpenAPI vigentes y los dobles de bus/telemetría demuestran publicación única, no-publicación en rechazo y observabilidad.

## Project Structure

### Documentation (this feature)

```text
specs/005-usecase-mediatr-integration/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── http-compatibility.md
│   └── mediation-contract.md
└── tasks.md                    # Creado posteriormente por /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── GtMotive.Estimate.Microservice.Domain/
│   ├── Vehicles/Events/                # VehicleCreated
│   └── Rentals/Events/                 # VehicleRented, VehicleReturned
├── GtMotive.Estimate.Microservice.ApplicationCore/
│   ├── UseCases/                       # contratos proporcionados
│   ├── Vehicles/{Create,List}/         # IRequest, handlers, results y use cases
│   └── Rentals/{Rent,Return}/           # IRequest, handlers, results y use cases
├── GtMotive.Estimate.Microservice.Api/
│   ├── Vehicles/{Create,List}/         # DTOs HTTP, controllers y presenters
│   ├── Rentals/{Rent,Return}/          # DTOs HTTP, controllers y presenters
│   └── ApiConfiguration.cs             # registro MediatR y presentación
├── GtMotive.Estimate.Microservice.Infrastructure/
│   └── InfrastructureConfiguration.cs  # ITelemetry e IBusFactory
└── GtMotive.Estimate.Microservice.Host/
    └── Program.cs                       # composición

test/
├── unit/GtMotive.Estimate.Microservice.UnitTests/Mediation/
├── functional/GtMotive.Estimate.Microservice.FunctionalTests/
│   ├── Mediation/
│   └── TestDoubles/                    # RecordingBusFactory/RecordingTelemetry
└── infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Mediation/
```

**Structure Decision**: Se conserva la arquitectura hexagonal. Mensajes, handlers, entradas, salidas y casos de uso viven en ApplicationCore; los eventos sin tecnología viven en Domain. Api solo mapea DTO HTTP a `IRequest<TResultado>` y presenta el resultado. Infrastructure aporta implementaciones de bus/telemetría y Host mantiene la composición.

## Design Sequence

1. Hacer que `CreateVehicleCommand`, `RentVehicleCommand`, `ReturnVehicleCommand` y una nueva `ListVehiclesQuery` implementen `IUseCaseInput`; hacer que los cuatro resultados implementen `IUseCaseOutput`.
2. Hacer que cada caso de uso implemente `IUseCase<TInput>` conservando `ExecuteAsync(..., CancellationToken)` como entrada tipada que devuelve el resultado. La implementación contractual delegará en la misma lógica sin duplicarla; handlers y pruebas usarán el método cancelable.
3. Definir en Domain `VehicleCreated`, `VehicleRented` y `VehicleReturned` con identificadores y marcas temporales no sensibles; no crear evento para `ListVehicles`.
4. Hacer que los comandos/query de ApplicationCore implementen `IRequest<TResultado>` y crear allí un handler por operación. Mantener las requests de Api como DTOs puros y mapearlas en cada controller.
5. En mutaciones, inspeccionar el resultado y solo para `Created`/`Returned` obtener `IBus` mediante `IBusFactory.GetClient(event.GetType())` y ejecutar un único `Send`. Propagar fallo; no reintentar dentro del handler.
6. Instrumentar los cuatro handlers con `ITelemetry`: evento `UseCaseCompleted` con `operation`/`outcome`, y métrica `UseCaseDurationMs`; medir en `finally`, clasificar éxito/rechazo/error/cancelación y excluir identificadores, cuerpos y excepciones.
7. Cambiar los controladores para inyectar `IMediator`, enviar el mensaje con el token HTTP y devolver el resultado presentado, manteniendo atributos, rutas, status y esquemas.
8. Completar DI para casos de uso, handlers, `IBusFactory` y `ITelemetry`; añadir prueba de composición de exactamente un handler por mensaje.
9. Añadir pruebas unitarias de conformidad/coordinación, funcionales de handler+caso de uso+dobles e infraestructura HTTP/Host para publicación única, ausencia en rechazo y telemetría.
10. Ejecutar restore, build/analyzers, las tres suites y las comprobaciones local/Docker de [quickstart.md](quickstart.md).

## Complexity Tracking

No hay violaciones constitucionales que requieran justificación.
