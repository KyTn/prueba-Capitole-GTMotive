# Tasks: Listar vehículos de la flota

**Input**: Design documents from `/specs/002-listar-vehiculos/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Las pruebas son obligatorias porque el spec solicita explícitamente cobertura unitaria sin infraestructura, funcional sin Host e infraestructura a nivel Host.

**Organization**: Las tareas se agrupan por historia para entregar primero el listado completo como MVP y después cerrar explícitamente el comportamiento de flota vacía.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo porque trabaja en archivos distintos y no depende de otra tarea incompleta.
- **[Story]**: Relaciona la tarea con una historia del spec.
- Todas las tareas incluyen rutas concretas.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar el proyecto de pruebas aisladas para ejercitar ApplicationCore sin añadir paquetes ni proyectos.

- [ ] T001 Añadir la referencia a `src/GtMotive.Estimate.Microservice.ApplicationCore/GtMotive.Estimate.Microservice.ApplicationCore.csproj` en `test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establecer el contrato compartido de vehículo y la capacidad de lectura que necesitan ambas historias.

**CRITICAL**: Ninguna historia puede implementarse hasta completar esta fase.

- [ ] T002 Mover `VehicleDto` al espacio compartido en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/VehicleDto.cs` y actualizar su uso en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/Create/CreateVehicleUseCase.cs`
- [ ] T003 Ampliar `IVehicleRepository` con `GetAllAsync(CancellationToken)` materializado y de solo lectura en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/IVehicleRepository.cs`
- [ ] T004 Añadir una vía de rehidratación que preserve las invariantes ya validadas de vehículos persistidos en `src/GtMotive.Estimate.Microservice.Domain/Vehicles/Vehicle.cs`
- [ ] T005 Añadir el mapeo inverso completo `VehicleDocument` a `Vehicle` en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/VehicleMapper.cs`
- [ ] T006 Actualizar el doble funcional para implementar `GetAllAsync` sin modificar el estado en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryVehicleRepository.cs`
- [ ] T007 Actualizar el repositorio controlado del Host para implementar `GetAllAsync` en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/VehicleApiFactory.cs`

**Checkpoint**: El contrato de lectura compila en producción y en todos los dobles de prueba.

---

## Phase 3: User Story 1 - Consultar la flota disponible (Priority: P1) MVP

**Goal**: Devolver todos los vehículos registrados exactamente una vez, con sus cinco campos públicos, mediante `GET /vehicles`.

**Independent Test**: Registrar varios vehículos conocidos, invocar el listado y comprobar que cada uno aparece una sola vez con identificador, matrícula, marca, modelo y fecha de fabricación, sin alterar el repositorio.

### Tests for User Story 1

> Escribir primero estas pruebas y confirmar que fallan antes de implementar la historia.

- [ ] T008 [P] [US1] Crear la prueba unitaria aislada del método de listado con un stub determinista y propagación de cancelación en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Vehicles/ListVehiclesUseCaseTests.cs`
- [ ] T009 [P] [US1] Crear la prueba funcional sin Host para listado completo, ausencia de duplicados, estado inalterado y fallo de lectura sin resultado parcial en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/ListVehiclesUseCaseTests.cs`
- [ ] T010 [P] [US1] Crear la prueba de infraestructura de `GET /vehicles` para una flota poblada, verificando `200`, JSON y campos obligatorios a través de Host en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/ListVehiclesEndpointTests.cs`

### Implementation for User Story 1

- [ ] T011 [P] [US1] Crear el resultado inmutable y no nulo `ListVehiclesResult` en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/List/ListVehiclesResult.cs`
- [ ] T012 [US1] Implementar `ListVehiclesUseCase` con proyección completa a `VehicleDto`, `CancellationToken` y log agregado sin datos de vehículo en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/List/ListVehiclesUseCase.cs`
- [ ] T013 [P] [US1] Implementar `GetAllAsync` con materialización completa o excepción en `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Vehicles/MongoVehicleRepository.cs`
- [ ] T014 [P] [US1] Crear el contrato de respuesta HTTP de vehículo reutilizando los cinco campos públicos en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesResponse.cs`
- [ ] T015 [US1] Crear el presenter que convierte el resultado completo en una respuesta `200` sin streaming en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesPresenter.cs`
- [ ] T016 [US1] Implementar `GET /vehicles`, documentación de respuestas y propagación de cancelación en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesController.cs`
- [ ] T017 [US1] Registrar `ListVehiclesUseCase` conservando el registro de creación existente en `src/GtMotive.Estimate.Microservice.ApplicationCore/ApplicationConfiguration.cs`

**Checkpoint**: US1 devuelve una flota poblada completa mediante el caso de uso aislado, la integración sin Host y HTTP/Host.

---

## Phase 4: User Story 2 - Consultar una flota vacía (Priority: P2)

**Goal**: Representar una flota sin vehículos como una consulta satisfactoria `200` con `[]`, nunca como `null`, `204` o `404`.

**Independent Test**: Ejecutar el caso de uso y `GET /vehicles` sobre repositorios vacíos y verificar una colección no nula con cero elementos y ausencia de cambios.

### Tests for User Story 2

> Escribir primero estas pruebas y confirmar que fallan si la colección vacía no se conserva en cada frontera.

- [ ] T018 [P] [US2] Crear la prueba unitaria de resultado vacío no nulo en `test/unit/GtMotive.Estimate.Microservice.UnitTests/Vehicles/ListVehiclesEmptyFleetTests.cs`
- [ ] T019 [P] [US2] Crear la prueba funcional sin Host de flota vacía y repositorio inalterado en `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Vehicles/ListVehiclesEmptyFleetTests.cs`
- [ ] T020 [P] [US2] Crear la prueba de infraestructura que exige `200 application/json` con `[]` a través de Host en `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Vehicles/ListVehiclesEmptyFleetEndpointTests.cs`

### Implementation for User Story 2

- [ ] T021 [US2] Garantizar que el resultado, presenter y serialización preservan una colección vacía no nula en `src/GtMotive.Estimate.Microservice.ApplicationCore/Vehicles/List/ListVehiclesResult.cs` y `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesPresenter.cs`

**Checkpoint**: US2 distingue de forma verificable una flota vacía de un fallo en los tres niveles de prueba.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Cerrar documentación, compatibilidad, calidad y reproducibilidad para ambas historias.

- [ ] T022 [P] Actualizar la documentación Swagger del recurso compartido GET/POST y comprobarla contra `specs/002-listar-vehiculos/contracts/openapi.yaml` en `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesController.cs`
- [ ] T023 [P] Revisar el quickstart con las rutas y resultados finales de las suites en `specs/002-listar-vehiculos/quickstart.md`
- [ ] T024 Ejecutar restore, build Release, analizadores y las suites unit, functional e infrastructure definidas en `specs/002-listar-vehiculos/quickstart.md`
- [ ] T025 Validar `docker compose config`, build, arranque y `GET /vehicles` contenedorizado, documentando la evidencia en `specs/002-listar-vehiculos/quickstart.md`
- [ ] T026 Revisar compatibilidad de `POST /vehicles`, dirección de dependencias, logs sin datos sensibles y ausencia de secretos usando `specs/002-listar-vehiculos/plan.md` como checklist técnico

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias; comienza inmediatamente.
- **Foundational (Phase 2)**: Depende de T001 y bloquea ambas historias.
- **User Story 1 (Phase 3)**: Depende de Phase 2 y entrega el MVP.
- **User Story 2 (Phase 4)**: Depende del endpoint y caso de uso de US1, pero sus criterios se validan de forma independiente sobre una flota vacía.
- **Polish (Phase 5)**: Depende de las historias que se vayan a entregar; para cerrar la feature deben completarse US1 y US2.

### User Story Dependencies

```text
Setup → Foundation → US1 (MVP) → US2 → Polish
```

- **US1 (P1)**: No depende de otra historia; sólo de la base compartida.
- **US2 (P2)**: Reutiliza el caso de uso y endpoint de US1, pero aporta y verifica una semántica de resultado distinta.

### Within Each User Story

- Las pruebas se escriben y se ejecutan en rojo antes de la implementación.
- El resultado y el caso de uso preceden al presenter y al endpoint.
- La lectura de Infrastructure y el contrato Api pueden progresar en paralelo tras fijar el puerto.
- Cada checkpoint exige pasar las pruebas específicas de la historia antes de avanzar.

### Parallel Opportunities

- T008, T009 y T010 pueden escribirse en paralelo.
- T011, T013 y T014 trabajan en proyectos y archivos distintos tras completar Foundation.
- T018, T019 y T020 pueden escribirse en paralelo.
- T022 y T023 pueden ejecutarse en paralelo.
- Tras implementar US1, las comprobaciones aisladas de cada suite pueden ejecutarse simultáneamente.

---

## Parallel Example: User Story 1

```text
Task T008: Crear la prueba unitaria en test/unit/.../ListVehiclesUseCaseTests.cs
Task T009: Crear la prueba funcional en test/functional/.../ListVehiclesUseCaseTests.cs
Task T010: Crear la prueba Host en test/infrastructure/.../ListVehiclesEndpointTests.cs

Después de fijar el resultado:
Task T013: Implementar la lectura MongoDB en src/.../MongoVehicleRepository.cs
Task T014: Crear la respuesta HTTP en src/.../ListVehiclesResponse.cs
```

## Parallel Example: User Story 2

```text
Task T018: Probar colección vacía en Unit
Task T019: Probar colección vacía sin Host en Functional
Task T020: Probar HTTP 200 + [] en Infrastructure
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Setup.
2. Completar Foundation.
3. Escribir T008–T010 y confirmar los fallos esperados.
4. Implementar T011–T017.
5. Ejecutar las pruebas de US1 y detenerse para validar el listado poblado.

### Incremental Delivery

1. Setup + Foundation establecen un puerto y modelo compartidos.
2. US1 entrega el listado completo poblado como MVP.
3. US2 fija la semántica uniforme de flota vacía.
4. Polish verifica compatibilidad, documentación y ejecución local/contenedorizada.

### Parallel Team Strategy

1. El equipo completa T001–T007 secuencialmente donde hay contratos compartidos.
2. Tras Foundation, tres personas pueden preparar en paralelo Unit, Functional e Infrastructure.
3. ApplicationCore, Infrastructure y el contrato de respuesta Api pueden avanzar en paralelo una vez fijado `ListVehiclesResult`.
4. La integración final del controlador y DI se realiza antes de ejecutar el checkpoint de US1.

## Notes

- `[P]` sólo aparece cuando las tareas modifican archivos distintos y no dependen de trabajo incompleto.
- `[US1]` y `[US2]` proporcionan trazabilidad directa con `spec.md`.
- Las pruebas de Unit, Functional e Infrastructure son distintas y ninguna cuenta en más de una categoría.
- No se añaden paquetes ni proyectos nuevos.
- Cada tarea puede completarse usando los documentos de `specs/002-listar-vehiculos/` sin decisiones adicionales.
