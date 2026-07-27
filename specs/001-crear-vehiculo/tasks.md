# Tasks: Crear vehículo en la flota

**Input**: Design documents from `/specs/001-crear-vehiculo/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Las pruebas son obligatorias y se escriben antes de la implementación correspondiente. Unitarias, funcionales sin Host e infraestructura con Host permanecen en proyectos distintos.

**Organization**: Las tareas se agrupan por historia para permitir implementación y verificación independientes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo al afectar archivos distintos y no depender de otra tarea incompleta.
- **[Story]**: Historia de usuario cubierta por la tarea.
- Cada tarea incluye rutas exactas.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Crear los proyectos de prueba y conectarlos a la solución con referencias que impongan las fronteras arquitectónicas.

- [X] T001 Crear el proyecto xUnit unitario con referencia exclusiva a Domain en `test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj`
- [X] T002 [P] Crear el proyecto xUnit funcional con referencias a Domain, ApplicationCore e Infrastructure, excluyendo Host, en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/GtMotive.Estimate.Microservice.FunctionalTests.csproj`
- [X] T003 [P] Crear el proyecto xUnit de infraestructura con referencias a Host y Infrastructure y soporte TestHost en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/GtMotive.Estimate.Microservice.InfrastructureTests.csproj`
- [X] T004 Incorporar los tres proyectos de prueba a `src/microservice.sln`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Definir contratos compartidos y configuración mínima antes de implementar las historias.

**CRITICAL**: Ninguna historia comienza hasta completar esta fase.

- [X] T005 [P] Definir `IClock` y la implementación de reloj del sistema en `src/GtMotive.Estimate.Microservice.ApplicationCore/Common/Time/IClock.cs` y `src/GtMotive.Estimate.Microservice.Infrastructure/Time/SystemClock.cs`
- [X] T006 [P] Definir `IVehicleRepository`, incluida la propagación de `CancellationToken`, en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/IVehicleRepository.cs`
- [X] T007 [P] Definir los modelos `CreateVehicleCommand`, `CreateVehicleResult` y `VehicleDto` en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/`
- [X] T008 Crear dobles deterministas de reloj y repositorio para pruebas funcionales sin Host en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/FixedClock.cs` y `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryVehicleRepository.cs`
- [X] T009 Configurar la sección y nombre de colección `vehicles` en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Settings/MongoDbSettings.cs` y `src/GtMotive.Estimate.Microservice.Host/appsettings.Development.json`

**Checkpoint**: Los límites de aplicación, tiempo y persistencia están listos y no exponen ASP.NET Core ni MongoDB al dominio.

---

## Phase 3: User Story 1 - Incorporar un vehículo válido (Priority: P1) MVP

**Goal**: Registrar un vehículo válido y devolver `201`, identificador, datos normalizados y `Location`.

**Independent Test**: Enviar un alta con matrícula nueva y fecha dentro de los últimos cinco años; debe persistirse exactamente un vehículo y devolverse su representación.

### Tests for User Story 1

- [X] T010 [P] [US1] Escribir pruebas unitarias fallidas de normalización de matrícula y creación válida de vehículo, incluido exactamente cinco años, en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Vehicles/VehicleTests.cs`
- [X] T011 [P] [US1] Escribir prueba funcional fallida del caso de uso válido con reloj y repositorio en memoria en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/CreateVehicleUseCaseTests.cs`
- [X] T012 [P] [US1] Escribir prueba de infraestructura fallida para `POST /vehicles` con respuesta `201`, `Location`, cuerpo y persistencia observable en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/CreateVehicleEndpointTests.cs`

### Implementation for User Story 1

- [X] T013 [P] [US1] Implementar el value object normalizado `RegistrationNumber` en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/RegistrationNumber.cs`
- [X] T014 [US1] Implementar `Vehicle.Create` para el camino válido y el límite inclusivo de cinco años en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/Vehicle.cs`
- [X] T015 [US1] Implementar `CreateVehicleUseCase` para generar el identificador, obtener la fecha del reloj, persistir y devolver el DTO en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleUseCase.cs`
- [X] T016 [P] [US1] Implementar el documento y mapeo MongoDB de Vehicle en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/VehicleDocument.cs` y `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/VehicleMapper.cs`
- [X] T017 [US1] Implementar la inserción y consulta básica del repositorio MongoDB en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/MongoVehicleRepository.cs`
- [X] T018 [P] [US1] Implementar request, response y presenter/mapeo del alta en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehicleRequest.cs`, `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehicleResponse.cs` y `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehiclePresenter.cs`
- [X] T019 [US1] Implementar `POST /vehicles`, propagación de cancelación y respuesta `201` con `Location` en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/VehiclesController.cs`
- [X] T020 [US1] Registrar caso de uso, reloj, repositorio y descubrimiento del endpoint en `src/GtMotive.Estimate.Microservice.ApplicationCore/ApplicationConfiguration.cs`, `src/GtMotive.Estimate.Microservice.Infrastructure/InfrastructureConfiguration.cs`, `src/GtMotive.Estimate.Microservice.Api/ApiConfiguration.cs` y `src/GtMotive.Estimate.Microservice.Host/Program.cs`

**Checkpoint**: US1 funciona de extremo a extremo para altas válidas y puede demostrarse sin depender de US2 o US3.

---

## Phase 4: User Story 2 - Impedir vehículos demasiado antiguos (Priority: P1)

**Goal**: Rechazar fechas futuras o con más de cinco años y preservar la flota sin cambios.

**Independent Test**: Ejecutar la matriz temporal con fecha controlada: dentro del límite y exactamente en él se aceptan; un día anterior y una fecha futura se rechazan sin persistencia.

### Tests for User Story 2

- [X] T021 [P] [US2] Ampliar las pruebas unitarias con un día fuera del límite, fecha futura y frontera de 29 de febrero en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Vehicles/VehicleTests.cs`
- [X] T022 [P] [US2] Añadir pruebas funcionales de rechazo por antigüedad y ausencia de cambios tras el rechazo en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/CreateVehicleAgeValidationTests.cs`
- [X] T023 [P] [US2] Añadir pruebas Host para `422 vehicle_too_old` y `400 future_manufacture_date` en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/CreateVehicleValidationEndpointTests.cs`

### Implementation for User Story 2

- [X] T024 [US2] Completar las invariantes de fecha futura, antigüedad y año bisiesto y sus errores de dominio en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/Vehicle.cs` y `src/GtMotive.Estimate.Microservice.Domain/Vehicles/VehicleValidationException.cs`
- [X] T025 [US2] Traducir los fallos temporales a resultados `InvalidInput` y `VehicleTooOld` sin invocar persistencia en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleUseCase.cs`
- [X] T026 [US2] Mapear los resultados temporales a `400` y `422` con códigos ProblemDetails estables en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehiclePresenter.cs`

**Checkpoint**: US2 protege completamente INV-001 en dominio, aplicación y contrato HTTP.

---

## Phase 5: User Story 3 - Rechazar altas inválidas o duplicadas (Priority: P2)

**Goal**: Rechazar datos obligatorios inválidos y matrículas duplicadas, incluso concurrentes, sin registros parciales.

**Independent Test**: Enviar entradas vacías, matrícula equivalente normalizada y dos altas concurrentes; los resultados deben ser `400`/`409` y debe existir como máximo un vehículo.

### Tests for User Story 3

- [X] T027 [P] [US3] Añadir pruebas unitarias fallidas para identificador vacío, matrícula, marca y modelo vacíos en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Vehicles/VehicleRequiredFieldsTests.cs`
- [X] T028 [P] [US3] Añadir pruebas funcionales fallidas para duplicado normalizado, no persistencia en entradas inválidas y traducción de colisión de repositorio en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/CreateVehicleConflictTests.cs`
- [X] T029 [P] [US3] Añadir pruebas Host fallidas para request inválido, duplicado y dos altas concurrentes con un único registro final en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/CreateVehicleConflictEndpointTests.cs`

### Implementation for User Story 3

- [X] T030 [P] [US3] Completar validaciones obligatorias y errores de `RegistrationNumber` en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/RegistrationNumber.cs`
- [X] T031 [US3] Completar validaciones de identificador, marca y modelo en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/Vehicle.cs`
- [X] T032 [US3] Incorporar comprobación de matrícula existente y resultado `VehicleAlreadyExists` en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleUseCase.cs`
- [X] T033 [US3] Crear de forma idempotente el índice único de matrícula y traducir la clave duplicada en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/MongoVehicleRepository.cs`
- [X] T034 [US3] Mapear validación de campos a `400` y duplicados a `409` sin detalles internos en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/CreateVehiclePresenter.cs`
- [X] T035 [US3] Añadir logs estructurados de resultado del alta sin datos sensibles en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleUseCase.cs`

**Checkpoint**: Las tres historias funcionan y las invariantes INV-002 e INV-003 resisten entradas inválidas y concurrencia.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cerrar contrato, reproducibilidad y puertas de calidad.

- [X] T036 [P] Actualizar documentación Swagger y verificar su correspondencia con `specs/001-crear-vehiculo/contracts/openapi.yaml` en `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/VehiclesController.cs`
- [X] T037 [P] Fijar una versión compatible de MongoDB y aislar la base de pruebas en `compose.yaml` y `src/GtMotive.Estimate.Microservice.Host/appsettings.Development.json`
- [X] T038 Resolver la referencia Visual Studio inexistente añadiendo el proyecto requerido o eliminando la propiedad obsoleta en `src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj`
- [X] T039 [P] Revisar exclusiones, imágenes oficiales, puertos y ausencia de secretos en `.dockerignore`, `Dockerfile` y `compose.yaml`
- [X] T040 Ejecutar restore, build, analizadores y los tres proyectos de prueba siguiendo `specs/001-crear-vehiculo/quickstart.md`
- [X] T041 Validar desde un entorno limpio el arranque local, `docker compose config`, build, endpoint y Swagger, y registrar ajustes finales en `specs/001-crear-vehiculo/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias.
- **Foundational (Phase 2)**: Depende de Setup y bloquea todas las historias.
- **US1 (Phase 3)**: Depende de Foundational y constituye el MVP.
- **US2 (Phase 4)**: Depende de los tipos de dominio y el flujo creados en US1.
- **US3 (Phase 5)**: Depende del alta base de US1; puede desarrollarse en paralelo con US2 después de US1.
- **Polish (Phase 6)**: Depende de todas las historias.

### User Story Dependency Graph

```text
Setup -> Foundational -> US1 (MVP) -> US2
                                  \-> US3
US2 + US3 -> Polish
```

### Within Each User Story

- Escribir primero las pruebas y comprobar que fallan por la capacidad ausente.
- Implementar value objects/entidades antes del caso de uso.
- Implementar el caso de uso antes de adaptadores HTTP.
- Verificar el checkpoint antes de avanzar a otra historia.

### Parallel Opportunities

- T002 y T003 pueden ejecutarse en paralelo tras T001.
- T005, T006 y T007 afectan contratos distintos y pueden ejecutarse en paralelo.
- En US1, T010-T012 pueden escribirse en paralelo; T013 y T016 también pueden avanzar en paralelo.
- En US2, T021-T023 pueden escribirse en paralelo.
- En US3, T027-T029 pueden escribirse en paralelo; T030 puede avanzar independientemente del adaptador de persistencia.
- US2 y US3 pueden ejecutarse en paralelo después de completar US1.
- T036, T037 y T039 pueden ejecutarse en paralelo antes de las verificaciones finales.

---

## Parallel Example: User Story 1

```text
Task T010: Unit tests in test/unit/GtMotive.Estimate.Microservice.UnitTests/Vehicles/VehicleTests.cs
Task T011: Functional test in test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/CreateVehicleUseCaseTests.cs
Task T012: Host test in test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/CreateVehicleEndpointTests.cs
```

## Parallel Example: User Story 2

```text
Task T021: Unit temporal-boundary matrix
Task T022: Functional rejection/no-change scenarios
Task T023: Host 400/422 contract scenarios
```

## Parallel Example: User Story 3

```text
Task T027: Unit required-field validation
Task T028: Functional duplicate/atomicity scenarios
Task T029: Host duplicate/concurrency scenarios
```

---

## Implementation Strategy

### MVP First

1. Completar Setup y Foundational.
2. Completar US1 con sus tres pruebas.
3. Detenerse y validar el alta válida de extremo a extremo.
4. Entregar o demostrar el MVP antes de ampliar reglas de rechazo.

### Incremental Delivery

1. US1 entrega creación válida.
2. US2 añade protección temporal sin cambiar el contrato satisfactorio.
3. US3 añade calidad de entrada y seguridad ante duplicados/concurrencia.
4. Polish cierra reproducibilidad, documentación y puertas de calidad.

### Parallel Team Strategy

Tras US1, un flujo puede completar US2 mientras otro completa US3. Dentro de cada historia, los tres niveles de pruebas pueden prepararse en paralelo porque viven en proyectos y archivos distintos.

## Notes

- `[P]` sólo aparece cuando la tarea puede avanzar sobre archivos distintos sin una dependencia incompleta.
- `[US1]`, `[US2]` y `[US3]` mantienen trazabilidad directa con `spec.md`.
- Ninguna prueba se contabiliza en más de una categoría.
- No se implementan consulta, modificación, eliminación, alquiler ni devolución.
