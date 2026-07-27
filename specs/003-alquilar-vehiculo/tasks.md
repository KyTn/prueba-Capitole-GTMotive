# Tasks: Alquilar un vehículo

**Input**: Design documents from `/specs/003-alquilar-vehiculo/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Las pruebas son obligatorias. Cada invariante debe tener cobertura unitaria y se mantienen separadas las pruebas funcionales sin Host y las pruebas de infraestructura a nivel Host.

**Organization**: Las tareas se agrupan por historia para que cada incremento pueda implementarse y verificarse de forma independiente.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede realizarse en paralelo porque afecta a archivos diferentes y no depende de otra tarea incompleta.
- **[Story]**: Historia de usuario cubierta (`US1`, `US2` o `US3`).
- Todas las tareas incluyen rutas de archivo exactas.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar configuración y estructura compartida sin introducir todavía comportamiento de historias.

- [ ] T001 Añadir `RentalsCollectionName` y la configuración del registro de personas en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Settings/MongoDbSettings.cs` y `src/GtMotive.Estimate.Microservice.Host/appsettings.json`
- [ ] T002 [P] Declarar `MongoDb__RentalsCollectionName` y la URL del registro de personas para ejecución reproducible en `compose.yaml`
- [ ] T003 [P] Crear las carpetas de feature con archivos namespace-placeholder eliminables en `src/GtMotive.Estimate.Microservice.Domain/Rentals/`, `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/`, `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/` y `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Definir las fronteras compartidas necesarias para todas las historias.

**⚠️ CRITICAL**: Ninguna historia puede completarse hasta finalizar esta fase.

- [ ] T004 [P] Crear el value object `PersonId` con validación de `Guid` no vacío en `src/GtMotive.Estimate.Microservice.Domain/Rentals/PersonId.cs`
- [ ] T005 [P] Definir `RentalStatus` con estados `Active` y `Closed` en `src/GtMotive.Estimate.Microservice.Domain/Rentals/RentalStatus.cs`
- [ ] T006 [P] Definir el puerto asíncrono `IPersonRegistry.ExistsAsync(PersonId, CancellationToken)` en `src/GtMotive.Estimate.Microservice.ApplicationCore/People/IPersonRegistry.cs`
- [ ] T007 [P] Ampliar `IVehicleRepository` con lectura asíncrona por identificador en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/IVehicleRepository.cs`
- [ ] T008 Definir `IRentalRepository`, `AddActiveRentalResult` y la creación atómica tipada en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/IRentalRepository.cs`
- [ ] T009 [P] Añadir una referencia temporal UTC controlable para el inicio del alquiler en `src/GtMotive.Estimate.Microservice.ApplicationCore/Common/Time/IClock.cs` y `src/GtMotive.Estimate.Microservice.Infrastructure/Time/SystemClock.cs`
- [ ] T010 [P] Crear `RentalDocument` y su representación de estado en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/RentalDocument.cs`
- [ ] T011 Implementar lectura por id en MongoDB y actualizar dobles existentes para el nuevo contrato en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/MongoVehicleRepository.cs`, `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryVehicleRepository.cs` y `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/VehicleApiFactory.cs`
- [ ] T012 Implementar el adaptador `HttpPersonRegistry` con cancelación y respuesta booleana sin almacenar datos personales en `src/GtMotive.Estimate.Microservice.Infrastructure/People/HttpPersonRegistry.cs` y `src/GtMotive.Estimate.Microservice.Infrastructure/People/PersonRegistrySettings.cs`
- [ ] T013 Registrar configuración, `IPersonRegistry`, cliente HTTP y fronteras iniciales de alquiler en `src/GtMotive.Estimate.Microservice.Infrastructure/InfrastructureConfiguration.cs` y `src/GtMotive.Estimate.Microservice.Api/DependencyInjection/UserInterfaceExtensions.cs`

**Checkpoint**: Los contratos compartidos compilan y las historias pueden desarrollarse sobre puertos estables.

---

## Phase 3: User Story 1 - Alquilar un vehículo disponible (Priority: P1) 🎯 MVP

**Goal**: Crear un alquiler activo para una persona y un vehículo existentes y libres, impidiendo que el vehículo quede alquilado por dos personas.

**Independent Test**: Partiendo de una persona conocida y un vehículo disponible, `POST /rentals` devuelve `201` con la asignación; un segundo intento sobre ese vehículo devuelve `409` y sólo existe el alquiler original.

### Tests for User Story 1

> Escribir primero estas pruebas y comprobar que fallan antes de implementar.

- [ ] T014 [P] [US1] Crear pruebas unitarias de `Rental.Create` para identificadores válidos, estado `Active`, inicio UTC y ausencia de fin en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/RentalTests.cs`
- [ ] T015 [P] [US1] Crear pruebas funcionales sin Host para alquiler válido y conflicto por vehículo ocupado en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Rentals/RentVehicleUseCaseTests.cs`
- [ ] T016 [P] [US1] Crear pruebas Host para `201`, cuerpo, `Location` y posterior `409` sobre el mismo vehículo en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentVehicleEndpointTests.cs`

### Implementation for User Story 1

- [ ] T017 [US1] Implementar el agregado `Rental.Create`, propiedades inmutables y validación base en `src/GtMotive.Estimate.Microservice.Domain/Rentals/Rental.cs`
- [ ] T018 [P] [US1] Crear `RentVehicleCommand`, `RentalDto` y los tipos de resultado de éxito/conflicto en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleCommand.cs`, `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/RentalDto.cs` y `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleResult.cs`
- [ ] T019 [US1] Implementar `RentVehicleUseCase` para comprobar persona/vehículo, crear el agregado, persistirlo atómicamente y registrar resultados sin datos personales en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleUseCase.cs`
- [ ] T020 [P] [US1] Implementar mapeo bidireccional entre agregado y documento en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/RentalMapper.cs`
- [ ] T021 [US1] Implementar `MongoRentalRepository` con inserción única e índice parcial exclusivo por vehículo activo, traduciendo duplicate-key a `VehicleConflict`, en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/MongoRentalRepository.cs`
- [ ] T022 [P] [US1] Crear `RentVehicleRequest`, `RentVehicleResponse` y presenter para `201`/`409` en `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehicleRequest.cs`, `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehicleResponse.cs` y `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehiclePresenter.cs`
- [ ] T023 [US1] Implementar `POST /rentals` con propagación de `CancellationToken` y metadata Swagger en `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentalsController.cs`
- [ ] T024 [US1] Registrar `RentVehicleUseCase` y `IRentalRepository` en `src/GtMotive.Estimate.Microservice.ApplicationCore/ApplicationConfiguration.cs` y `src/GtMotive.Estimate.Microservice.Infrastructure/InfrastructureConfiguration.cs`
- [ ] T025 [US1] Crear dobles thread-safe de alquiler/persona y ampliar la factory Host para sembrar personas y vehículos en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryRentalRepository.cs`, `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryPersonRegistry.cs` y `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentalApiFactory.cs`
- [ ] T026 [US1] Ejecutar y hacer pasar las pruebas US1 de `RentalTests.cs`, `RentVehicleUseCaseTests.cs` y `RentVehicleEndpointTests.cs`

**Checkpoint**: El alquiler válido y la exclusividad del vehículo funcionan de extremo a extremo como MVP.

---

## Phase 4: User Story 2 - Impedir varios alquileres por persona (Priority: P1)

**Goal**: Rechazar un segundo alquiler activo de la misma persona, incluso si dos solicitudes para vehículos distintos compiten concurrentemente.

**Independent Test**: Dos solicitudes concurrentes de una persona para vehículos diferentes producen exactamente un `201`, un `409`, un solo alquiler activo y dejan el otro vehículo disponible.

### Tests for User Story 2

> Escribir primero estas pruebas y comprobar que fallan antes de implementar.

- [ ] T027 [P] [US2] Añadir pruebas unitarias del resultado de conflicto de persona sin persistencia parcial en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/RentVehicleResultTests.cs`
- [ ] T028 [P] [US2] Añadir pruebas funcionales secuenciales y concurrentes para un activo máximo por persona y conservación del segundo vehículo en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Rentals/PersonActiveRentalConflictTests.cs`
- [ ] T029 [P] [US2] Añadir pruebas Host para segundo alquiler y dos solicitudes concurrentes de la misma persona en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/PersonActiveRentalConflictEndpointTests.cs`

### Implementation for User Story 2

- [ ] T030 [US2] Ampliar `MongoRentalRepository` con índice único parcial por persona activa y traducción determinista a `PersonConflict` en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/MongoRentalRepository.cs`
- [ ] T031 [US2] Ampliar el doble concurrente para arbitrar atómicamente conflictos por persona y vehículo en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryRentalRepository.cs` y `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentalApiFactory.cs`
- [ ] T032 [US2] Mapear `PersonConflict` a resultado de aplicación y `409 ProblemDetails` estable sin exponer el identificador personal en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleUseCase.cs` y `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehiclePresenter.cs`
- [ ] T033 [US2] Ejecutar y hacer pasar las pruebas US2 de `RentVehicleResultTests.cs`, `PersonActiveRentalConflictTests.cs` y `PersonActiveRentalConflictEndpointTests.cs`

**Checkpoint**: Las exclusividades por persona y vehículo resisten operaciones secuenciales y concurrentes.

---

## Phase 5: User Story 3 - Rechazar referencias inválidas (Priority: P2)

**Goal**: Rechazar identificadores inválidos y referencias inexistentes con errores diferenciados, sin crear alquileres ni cambiar disponibilidad.

**Independent Test**: Requests con UUID vacío/mal formado devuelven `400`; persona o vehículo desconocido devuelve `404`; en todos los casos el repositorio de alquileres permanece vacío.

### Tests for User Story 3

> Escribir primero estas pruebas y comprobar que fallan antes de implementar.

- [ ] T034 [P] [US3] Ampliar pruebas unitarias para `PersonId` y `Rental.Create` con identificadores vacíos en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/PersonIdTests.cs` y `test/unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/RentalTests.cs`
- [ ] T035 [P] [US3] Crear pruebas funcionales sin Host para persona inexistente, vehículo inexistente y ausencia de cambios parciales en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Rentals/RentVehicleNotFoundTests.cs`
- [ ] T036 [P] [US3] Crear pruebas Host para request mal formado, UUID vacío y respuestas `404` diferenciadas en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentVehicleValidationEndpointTests.cs`

### Implementation for User Story 3

- [ ] T037 [US3] Añadir resultados `InvalidInput`, `PersonNotFound` y `VehicleNotFound` con códigos estables en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleResult.cs`
- [ ] T038 [US3] Completar validación y orden de consultas para detenerse antes de persistir ante referencias inválidas/inexistentes en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleUseCase.cs`
- [ ] T039 [US3] Completar validación de transporte y mapeos `400`/`404`/`500` en `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentalsController.cs` y `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentVehiclePresenter.cs`
- [ ] T040 [US3] Ejecutar y hacer pasar las pruebas US3 de `PersonIdTests.cs`, `RentalTests.cs`, `RentVehicleNotFoundTests.cs` y `RentVehicleValidationEndpointTests.cs`

**Checkpoint**: Las tres historias funcionan independientemente y todos los rechazos conservan el estado previo.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cerrar documentación, observabilidad, compatibilidad y quality gates.

- [ ] T041 [P] Sincronizar el contrato implementado y ejemplos de error con `specs/003-alquilar-vehiculo/contracts/openapi.yaml` y la metadata Swagger de `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentalsController.cs`
- [ ] T042 [P] Actualizar configuración y guía reproducible del registro de personas y colección de alquileres en `specs/003-alquilar-vehiculo/quickstart.md`, `src/GtMotive.Estimate.Microservice.Host/appsettings.Development.json` y `compose.yaml`
- [ ] T043 Revisar logs estructurados para incluir resultado y `RentalId`/`VehicleId` sin `PersonId` ni datos sensibles en `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Rent/RentVehicleUseCase.cs`
- [ ] T044 Ejecutar restore, build Release, analizadores y los tres proyectos de prueba usando los comandos de `specs/003-alquilar-vehiculo/quickstart.md`
- [ ] T045 Validar `docker compose config`, build/startup con imágenes oficiales, `.dockerignore`, puertos, health dependencies y ausencia de secretos según `Dockerfile`, `compose.yaml` y `src/GtMotive.Estimate.Microservice.Host/Properties/launchSettings.json`
- [ ] T046 Revisar trazabilidad FR/INV/SC, dirección de dependencias y alcance sin devolución/pagos en `specs/003-alquilar-vehiculo/spec.md`, `specs/003-alquilar-vehiculo/plan.md` y `specs/003-alquilar-vehiculo/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias.
- **Foundational (Phase 2)**: Depende de Setup y bloquea todas las historias.
- **US1 (Phase 3)**: Depende de Foundation y entrega el MVP.
- **US2 (Phase 4)**: Depende de las fronteras y flujo creados en US1; añade la segunda exclusividad.
- **US3 (Phase 5)**: Depende del flujo de US1, pero sus pruebas pueden prepararse tras Foundation.
- **Polish (Phase 6)**: Depende de las historias que se incluyan en la entrega; para completar T3 requiere US1, US2 y US3.

### User Story Dependency Graph

```text
Setup → Foundation → US1 (MVP) → US2
                         └──────→ US3
US1 + US2 + US3 → Polish
```

### Within Each User Story

- Escribir las pruebas y confirmar que fallan antes de implementar.
- Modelo/contratos antes del caso de uso.
- Caso de uso y repositorio antes del endpoint.
- Implementación antes de ejecutar el checkpoint completo.
- La protección concurrente definitiva reside en `IRentalRepository`; las consultas previas no sustituyen la escritura atómica.

### Parallel Opportunities

- T002 y T003 pueden ejecutarse en paralelo tras T001.
- T004–T007, T009 y T010 afectan a archivos independientes; T008 consolida sus tipos.
- T014–T016 pueden escribirse en paralelo; T018, T020 y T022 también.
- T027–T029 pueden escribirse en paralelo.
- T034–T036 pueden escribirse en paralelo.
- US3 puede preparar sus pruebas en paralelo con US2 después de completar US1.
- T041 y T042 pueden ejecutarse en paralelo antes de los quality gates finales.

---

## Parallel Example: User Story 1

```text
Task T014: Pruebas unitarias de Rental.Create en RentalTests.cs
Task T015: Pruebas funcionales del caso de uso en RentVehicleUseCaseTests.cs
Task T016: Pruebas Host del contrato en RentVehicleEndpointTests.cs

Después de T017:
Task T018: Contratos de aplicación
Task T020: Mapper MongoDB
Task T022: Contratos y presenter HTTP
```

## Parallel Example: User Story 2

```text
Task T027: Pruebas unitarias del conflicto por persona
Task T028: Pruebas funcionales secuenciales y concurrentes
Task T029: Pruebas Host secuenciales y concurrentes
```

## Parallel Example: User Story 3

```text
Task T034: Pruebas unitarias de identificadores
Task T035: Pruebas funcionales de recursos inexistentes
Task T036: Pruebas Host de 400 y 404
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Completar Setup.
2. Completar Foundation.
3. Escribir las pruebas US1 y confirmar el fallo esperado.
4. Implementar US1 hasta que los tres niveles pasen.
5. Detenerse y demostrar `201` seguido de `409` para el mismo vehículo.

### Incremental Delivery

1. Setup + Foundation → fronteras estables.
2. US1 → alquiler válido y vehículo exclusivo (MVP).
3. US2 → una persona no puede mantener dos vehículos.
4. US3 → errores `400`/`404` y atomicidad de rechazos.
5. Polish → contrato, reproducibilidad y gates completos.

### Parallel Team Strategy

1. El equipo completa Setup y Foundation.
2. En cada historia, Unit, Functional e Infrastructure se preparan en paralelo.
3. Tras US1, una línea puede completar US2 mientras otra prepara US3.
4. La integración final espera a las tres historias y ejecuta todos los gates.

## Notes

- `[P]` sólo marca tareas sobre archivos distintos sin una dependencia incompleta.
- Cada tarea de historia incluye `[US1]`, `[US2]` o `[US3]`.
- Unit no usa infraestructura; Functional excluye Host; Infrastructure atraviesa HTTP/Host.
- No se implementan devolución, pagos, duración, reserva pendiente ni alta de personas.
- Los tests concurrentes deben verificar estado final, no depender del orden de finalización.
