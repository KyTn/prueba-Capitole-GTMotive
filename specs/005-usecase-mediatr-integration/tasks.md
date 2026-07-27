# Tasks: IntegraciÃ³n de casos de uso, MediatR, eventos y telemetrÃ­a

**Input**: Design documents from `/specs/005-usecase-mediatr-integration/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Obligatorios por la constituciÃ³n. Las pruebas se escriben primero y deben fallar antes de implementar cada historia.

**Organization**: Las tareas se agrupan por historia para permitir implementaciÃ³n y validaciÃ³n independientes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo porque afecta archivos distintos y no depende de otra tarea incompleta.
- **[Story]**: Historia de usuario cubierta por la tarea.
- Todas las tareas incluyen rutas exactas.

## Phase 1: Setup (Shared Test Structure)

**Purpose**: Preparar los proyectos de prueba para validar handlers y tipos de Api sin aÃ±adir paquetes.

- [x] T001 [P] AÃ±adir referencia al proyecto Api en `test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj`
- [x] T002 [P] AÃ±adir referencia al proyecto Api en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/GtMotive.Estimate.Microservice.FunctionalTests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Fijar vocabulario compartido y garantizar que MediatR descubre los handlers en el assembly correcto.

**âš ï¸ CRITICAL**: Esta fase bloquea las historias.

- [x] T003 Crear constantes compartidas para nombres de operaciÃ³n, resultados y seÃ±ales `UseCaseCompleted`/`UseCaseDurationMs` en `src/GtMotive.Estimate.Microservice.Api/UseCases/UseCaseTelemetry.cs`
- [x] T004 Confirmar y ajustar el registro de MediatR para descubrir exactamente los handlers de Api sin registrar handlers duplicados en `src/GtMotive.Estimate.Microservice.Api/ApiConfiguration.cs`

**Checkpoint**: Estructura de pruebas y descubrimiento de mensajes preparados.

---

## Phase 3: User Story 1 - Ejecutar operaciones mediante el mediador (Priority: P1) ðŸŽ¯ MVP

**Goal**: Los cuatro controllers dependen de `IMediator`, cada solicitud alcanza exactamente un handler y los contratos HTTP permanecen iguales.

**Independent Test**: Ejecutar los cuatro endpoints mediante Host y comprobar rutas, status, cuerpos y headers existentes; validar ademÃ¡s que cada mensaje resuelve un solo handler y ningÃºn controller recibe un UseCase.

### Tests for User Story 1

- [x] T005 [P] [US1] Escribir pruebas unitarias fallidas que verifiquen por reflexiÃ³n que los cuatro controllers dependen de `IMediator` y no de UseCases en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Mediation/ControllerMediationTests.cs`
- [x] T006 [P] [US1] Escribir pruebas funcionales fallidas de delegaciÃ³n y propagaciÃ³n del `CancellationToken` para los cuatro handlers en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Mediation/MediatRHandlerTests.cs`
- [x] T007 [P] [US1] Escribir una prueba de infraestructura fallida que resuelva exactamente un handler por mensaje desde Host en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Mediation/MediatRCompositionTests.cs`
- [x] T008 [P] [US1] Ampliar las pruebas HTTP de compatibilidad de creaciÃ³n/listado sin cambiar expectativas en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/CreateVehicleEndpointTests.cs` y `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/ListVehiclesEndpointTests.cs`
- [x] T009 [P] [US1] Ampliar las pruebas HTTP de compatibilidad de alquiler/devoluciÃ³n sin cambiar expectativas en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentVehicleEndpointTests.cs` y `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/ReturnVehicleEndpointTests.cs`

### Implementation for User Story 1

- [x] T010 [P] [US1] Convertir `CreateVehicleRequest` en mensaje `IRequest<IActionResult>` y crear `CreateVehicleHandler` que mapee, ejecute `CreateVehicleUseCase.ExecuteAsync` y presente el resultado en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehicleRequest.cs` y `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehicleHandler.cs`
- [x] T011 [P] [US1] Crear `ListVehiclesRequest` como `IRequest<IActionResult>` y `ListVehiclesHandler` que ejecute/presente el listado en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesRequest.cs` y `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesHandler.cs`
- [x] T012 [P] [US1] Convertir `RentVehicleRequest` en mensaje `IRequest<IActionResult>` y crear `RentVehicleHandler` que mapee, ejecute `RentVehicleUseCase.ExecuteAsync` y presente el resultado en `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehicleRequest.cs` y `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehicleHandler.cs`
- [x] T013 [P] [US1] Convertir `ReturnVehicleRequest` en mensaje `IRequest<IActionResult>` y crear `ReturnVehicleHandler` que mapee, ejecute `ReturnVehicleUseCase.ExecuteAsync` y presente el resultado en `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehicleRequest.cs` y `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehicleHandler.cs`
- [x] T014 [P] [US1] Sustituir la inyecciÃ³n directa del UseCase por `IMediator` y enviar `CreateVehicleRequest` con el token HTTP en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/VehiclesController.cs`
- [x] T015 [P] [US1] Sustituir la inyecciÃ³n directa del UseCase por `IMediator` y enviar `ListVehiclesRequest` con el token HTTP en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesController.cs`
- [x] T016 [P] [US1] Sustituir la inyecciÃ³n directa del UseCase por `IMediator` y enviar `RentVehicleRequest` con el token HTTP en `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentalsController.cs`
- [x] T017 [P] [US1] Sustituir la inyecciÃ³n directa del UseCase por `IMediator` y enviar `ReturnVehicleRequest` con el token HTTP en `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/RentalReturnsController.cs`
- [x] T018 [US1] Ejecutar las pruebas de T005â€“T009 y corregir exclusivamente la mediaciÃ³n/compatibilidad en `src/GtMotive.Estimate.Microservice.Api/ApiConfiguration.cs`

**Checkpoint**: US1 es desplegable como MVP; los cuatro flujos usan MediatR sin cambios HTTP.

---

## Phase 4: User Story 2 - Reutilizar casos de uso mediante contratos comunes (Priority: P2)

**Goal**: Todos los inputs, outputs y UseCases alcanzables desde controllers cumplen las interfaces proporcionadas, y los handlers solo delegan.

**Independent Test**: Una prueba de conformidad verifica las doce relaciones de tipos y ejecuta cada implementaciÃ³n de `IUseCase<TInput>` sin duplicaciÃ³n de reglas ni dependencia MediatR en ApplicationCore/Domain.

### Tests for User Story 2

- [x] T019 [P] [US2] Escribir pruebas unitarias fallidas de conformidad para los cuatro inputs, cuatro outputs y cuatro UseCases en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Mediation/UseCaseContractTests.cs`
- [x] T020 [P] [US2] Escribir pruebas funcionales fallidas que comparen la ejecuciÃ³n contractual y tipada de cada UseCase sobre los mismos dobles en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Mediation/UseCaseContractExecutionTests.cs`

### Implementation for User Story 2

- [x] T021 [P] [US2] Implementar `IUseCaseInput` en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleCommand.cs`, `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleCommand.cs` y `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleCommand.cs`
- [x] T022 [P] [US2] Crear `ListVehiclesQuery : IUseCaseInput` y adaptar el mÃ©todo tipado de listado en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/List/ListVehiclesQuery.cs` y `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/List/ListVehiclesUseCase.cs`
- [x] T023 [P] [US2] Implementar `IUseCaseOutput` en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleResult.cs`, `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/List/ListVehiclesResult.cs`, `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleResult.cs` y `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleResult.cs`
- [x] T024 [P] [US2] Implementar `IUseCase<CreateVehicleCommand>` delegando en la misma lÃ³gica tipada en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleUseCase.cs`
- [x] T025 [P] [US2] Implementar `IUseCase<RentVehicleCommand>` delegando en la misma lÃ³gica tipada en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleUseCase.cs`
- [x] T026 [P] [US2] Implementar `IUseCase<ReturnVehicleCommand>` delegando en la misma lÃ³gica tipada en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleUseCase.cs`
- [x] T027 [US2] Registrar los cuatro UseCases por tipo concreto y por contrato sin duplicar instancias scoped en `src/GtMotive.Estimate.Microservice.ApplicationCore/ApplicationConfiguration.cs`
- [x] T028 [US2] Actualizar `ListVehiclesHandler` para construir `ListVehiclesQuery` y validar T019â€“T020 sin introducir MediatR en ApplicationCore/Domain en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesHandler.cs`

**Checkpoint**: US2 demuestra conformidad contractual y preserva los resultados/cancelaciÃ³n existentes.

---

## Phase 5: User Story 3 - Publicar resultados de dominio y observar operaciones (Priority: P3)

**Goal**: Las tres mutaciones exitosas envÃ­an exactamente un evento mediante `IBusFactory`; rechazos no envÃ­an; los cuatro handlers registran resultado y duraciÃ³n sin datos sensibles.

**Independent Test**: Ejecutar Ã©xito, rechazo, excepciÃ³n de bus y cancelaciÃ³n con dobles registradores; comprobar eventos, orden, telemetrÃ­a y que Host resuelve las implementaciones activa/no-op.

### Tests for User Story 3

- [x] T029 [P] [US3] Crear dobles thread-safe de `IBus`, `IBusFactory` e `ITelemetry` en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/RecordingBus.cs`, `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/RecordingBusFactory.cs` y `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/RecordingTelemetry.cs`
- [x] T030 [P] [US3] Escribir pruebas unitarias fallidas de inmutabilidad y payload no sensible para los tres eventos en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Mediation/DomainEventTests.cs`
- [x] T031 [P] [US3] Escribir pruebas funcionales fallidas de publicaciÃ³n Ãºnica para creaciÃ³n, alquiler y devoluciÃ³n y ausencia de publicaciÃ³n para listado/rechazos en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Mediation/DomainEventPublishingTests.cs`
- [x] T032 [P] [US3] Escribir pruebas funcionales fallidas de telemetrÃ­a success/rejected/error/cancelled, duraciÃ³n y ausencia de datos sensibles en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Mediation/UseCaseTelemetryTests.cs`
- [x] T033 [P] [US3] Escribir pruebas de infraestructura fallidas para resolver `IBusFactory`, `ITelemetry` y handlers desde Host en Development y Production en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Mediation/ObservabilityCompositionTests.cs`

### Implementation for User Story 3

- [x] T034 [P] [US3] Crear el evento inmutable `VehicleCreated` en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/Events/VehicleCreated.cs`
- [x] T035 [P] [US3] Crear el evento inmutable `VehicleRented` en `src/GtMotive.Estimate.Microservice.Domain/Rentals/Events/VehicleRented.cs`
- [x] T036 [P] [US3] Crear el evento inmutable `VehicleReturned` en `src/GtMotive.Estimate.Microservice.Domain/Rentals/Events/VehicleReturned.cs`
- [x] T037 [US3] Implementar el adaptador de bus por defecto y la selecciÃ³n de cliente por tipo en `src/GtMotive.Estimate.Microservice.Infrastructure/Messaging/BusFactory.cs` y `src/GtMotive.Estimate.Microservice.Infrastructure/Messaging/Bus.cs`
- [x] T038 [US3] Registrar `IBusFactory` y su cliente de bus junto con `ITelemetry` activa/no-op en `src/GtMotive.Estimate.Microservice.Infrastructure/InfrastructureConfiguration.cs`
- [x] T039 [P] [US3] Publicar `VehicleCreated` solo tras `Created` e instrumentar resultado/duraciÃ³n/fallo en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehicleHandler.cs`
- [x] T040 [P] [US3] Instrumentar listado sin publicar evento en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesHandler.cs`
- [x] T041 [P] [US3] Publicar `VehicleRented` solo tras `Created` e instrumentar resultado/duraciÃ³n/fallo en `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehicleHandler.cs`
- [x] T042 [P] [US3] Publicar `VehicleReturned` solo tras `Returned` e instrumentar resultado/duraciÃ³n/fallo en `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehicleHandler.cs`
- [x] T043 [US3] Configurar dobles de bus/telemetrÃ­a en las factories HTTP sin alterar repositorios ni relojes en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/VehicleApiFactory.cs` y `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentalApiFactory.cs`
- [x] T044 [US3] Ejecutar T030â€“T033 y corregir orden de Ã©xitoâ†’eventoâ†’presentaciÃ³n y telemetrÃ­a final en `src/GtMotive.Estimate.Microservice.Api/UseCases/UseCaseTelemetry.cs`

**Checkpoint**: US3 publica e instrumenta todos los resultados exigidos sin cambiar reglas ni HTTP.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: ValidaciÃ³n integral, documentaciÃ³n y reproducibilidad.

- [x] T045 [P] Actualizar Swagger/XML comments para reflejar contratos HTTP sin cambios y mediaciÃ³n interna en `src/GtMotive.Estimate.Microservice.Api/ApiConfiguration.cs`
- [x] T046 [P] Revisar trazas y eventos para excluir IDs, payloads, secretos y mensajes de excepciÃ³n segÃºn `specs/005-usecase-mediatr-integration/data-model.md`
- [x] T047 Ejecutar restore, build con analizadores y las suites unit/functional/infrastructure descritas en `specs/005-usecase-mediatr-integration/quickstart.md`
- [x] T048 Ejecutar las comprobaciones estÃ¡ticas de dependencias y controllers descritas en `specs/005-usecase-mediatr-integration/quickstart.md`
- [x] T049 Validar arranque local y los cuatro endpoints contra los contratos enlazados en `specs/005-usecase-mediatr-integration/contracts/http-compatibility.md`
- [x] T050 Validar `docker compose up --build`, los cuatro endpoints, imÃ¡genes oficiales, `.dockerignore`, configuraciÃ³n/puertos y ausencia de secretos siguiendo `specs/005-usecase-mediatr-integration/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias; T001 y T002 son paralelas.
- **Foundational (Phase 2)**: Depende de Setup y bloquea las historias.
- **US1 (Phase 3)**: Depende de Foundational; entrega el MVP de mediaciÃ³n y compatibilidad.
- **US2 (Phase 4)**: Depende de Foundational y puede desarrollarse en paralelo con US1, pero T028 se integra con T011.
- **US3 (Phase 5)**: Depende de los handlers de US1; sus eventos y dobles pueden comenzar tras Foundational.
- **Polish (Phase 6)**: Depende de las historias incluidas en la entrega.

### User Story Dependencies

```text
Setup â†’ Foundational â”€â”¬â†’ US1 (MVP) â”€â”€â”€â”€â”€â†’ US3
                     â””â†’ US2 â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                                      â†“
                                   Polish
```

- **US1**: Independiente tras Foundational.
- **US2**: Independiente para contratos; T028 espera el handler de listado de US1.
- **US3**: Eventos/dobles independientes, pero integraciÃ³n espera los handlers de US1 y los resultados contractuales de US2.

### Within Each User Story

- Escribir y ejecutar primero las pruebas para observar el fallo.
- Definir mensajes/eventos antes de handlers.
- Implementar handlers antes de cambiar controllers o composiciÃ³n.
- Ejecutar el checkpoint completo antes de avanzar.

### Parallel Opportunities

- T001â€“T002.
- T005â€“T009, despuÃ©s T010â€“T017.
- T019â€“T020, despuÃ©s T021â€“T026.
- T029â€“T033 y T034â€“T036.
- T039â€“T042 una vez disponibles eventos, bus y contratos.
- T045â€“T046 despuÃ©s de todas las historias.

---

## Parallel Example: User Story 1

```text
T005 ControllerMediationTests
T006 MediatRHandlerTests
T007 MediatRCompositionTests
T008 Vehicle HTTP compatibility
T009 Rental HTTP compatibility

DespuÃ©s:
T010 CreateVehicle flow
T011 ListVehicles flow
T012 RentVehicle flow
T013 ReturnVehicle flow
```

## Parallel Example: User Story 2

```text
T019 UseCaseContractTests
T020 UseCaseContractExecutionTests

DespuÃ©s:
T021 inputs
T022 list query
T023 outputs
T024/T025/T026 use cases
```

## Parallel Example: User Story 3

```text
T029 recording doubles
T030 domain event tests
T031 publishing tests
T032 telemetry tests
T033 Host composition tests
T034/T035/T036 domain events
```

---

## Implementation Strategy

### MVP First

1. Completar Setup y Foundational.
2. Completar US1.
3. Detenerse y validar los cuatro endpoints, la resoluciÃ³n de handlers y la compatibilidad HTTP.

### Incremental Delivery

1. **US1**: Controllers mediados y contratos externos estables.
2. **US2**: Contratos comunes verificables en ApplicationCore.
3. **US3**: Eventos y observabilidad completos.
4. **Polish**: Gates locales y Docker.

### Parallel Team Strategy

Tras Foundational, un equipo puede preparar mediaciÃ³n/HTTP (US1), otro contratos de ApplicationCore (US2) y otro eventos/dobles (parte independiente de US3). La integraciÃ³n de US3 comienza cuando US1 y US2 estÃ©n estables.

## Notes

- `[P]` indica archivos distintos y ausencia de dependencia inmediata.
- Cada tarea conserva reglas de negocio en Domain/ApplicationCore.
- No aÃ±adir MediatR a ApplicationCore ni Domain.
- No aÃ±adir outbox, reintentos duraderos, endpoints o cambios MongoDB.
- Realizar commits por tarea o grupo lÃ³gico despuÃ©s de que sus pruebas pasen.

