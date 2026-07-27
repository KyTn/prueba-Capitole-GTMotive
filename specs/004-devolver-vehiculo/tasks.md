# Tasks: Devolver un vehÃ­culo

**Input**: Design documents from `/specs/004-devolver-vehiculo/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Tests are mandatory for T4. The work includes isolated unit coverage, functional integration excluding Host, and infrastructure coverage through Host/HTTP.

**Organization**: Tasks are grouped by user story so each increment can be implemented and verified independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it changes different files and has no dependency on an incomplete task.
- **[Story]**: Maps the task to US1, US2, or US3.
- Every task names the exact target file.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing T3 baseline and establish the T4 contract before changing behavior.

- [x] T001 Verify the existing rental baseline with `dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests/GtMotive.Estimate.Microservice.UnitTests.csproj`, `dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests/GtMotive.Estimate.Microservice.FunctionalTests.csproj`, and `dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/GtMotive.Estimate.Microservice.InfrastructureTests.csproj`, recording any pre-existing failure in `specs/004-devolver-vehiculo/quickstart.md`
- [x] T002 [P] Reconcile the planned `POST /rentals/returns` schemas and response mappings with the existing rental API conventions in `specs/004-devolver-vehiculo/contracts/openapi.yaml`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add the shared domain state, application contracts, and persistence semantics required by every return scenario.

**âš ï¸ CRITICAL**: No user-story implementation starts until this phase is complete.

- [x] T003 Add failing unit tests for `Rental.Return`, `EndedAt` rehydration, repeated closure, and end-before-start rejection in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/RentalReturnTests.cs`
- [x] T004 Implement the `Active` to `Closed` transition, immutable `EndedAt`, temporal validation, and compatible rehydration in `src/GtMotive.Estimate.Microservice.Domain/Rentals/Rental.cs`
- [x] T005 [P] Extend the rental transfer representation with nullable `EndedAt` in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/RentalDto.cs`
- [x] T006 Define active-rental lookup and typed compare-and-set close results on `IRentalRepository` in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/IRentalRepository.cs`
- [x] T007 [P] Add `EndedAt` as an optional UTC field for backward-compatible documents in `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/RentalDocument.cs`
- [x] T008 Update both mapping directions for `EndedAt` and the `Closed` state in `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/RentalMapper.cs`
- [x] T009 Implement active lookup plus atomic close filtered by rental, person, vehicle, and `Status=Active` in `src/GtMotive.Estimate.Microservice.Infrastructure/MongoDb/Rentals/MongoRentalRepository.cs`
- [x] T010 Implement thread-safe active lookup and conditional close semantics for functional tests in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/InMemoryRentalRepository.cs`
- [x] T011 Implement the equivalent thread-safe conditional close and seeding support for Host tests in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/RentalApiFactory.cs`

**Checkpoint**: The domain and both production/test persistence boundaries can close an active rental exactly once.

---

## Phase 3: User Story 1 - Devolver un vehÃ­culo alquilado (Priority: P1) ðŸŽ¯ MVP

**Goal**: Close the active rental owned by the requesting person, record the return time, and make both person and vehicle available.

**Independent Test**: Seed one active rental, submit the matching person and vehicle, and verify one `Closed` rental with an immutable `EndedAt`, no active assignment, and HTTP `200`.

### Tests for User Story 1

> Write these tests first and confirm that they fail before implementing the story.

- [x] T012 [P] [US1] Add functional success coverage proving the rental closes and the person and vehicle can be rented again in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Rentals/ReturnVehicleUseCaseTests.cs`
- [x] T013 [P] [US1] Add Host-level contract coverage for `POST /rentals/returns` returning `200` with `closed` and `endedAt` in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/ReturnVehicleEndpointTests.cs`
- [x] T014 [P] [US1] Define the return command and success/error result types in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleCommand.cs` and `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleResult.cs`

### Implementation for User Story 1

- [x] T015 [US1] Implement input validation, reference checks, active-rental lookup, domain transition, conditional persistence, cancellation, and structured outcome logging in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleUseCase.cs`
- [x] T016 [P] [US1] Add return request and closed-rental response transport models in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehicleRequest.cs` and `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehicleResponse.cs`
- [x] T017 [US1] Map successful and expected return outcomes to `200`, `400`, `404`, and `409` Problem Details in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehiclePresenter.cs`
- [x] T018 [US1] Expose `POST /rentals/returns`, propagate the `CancellationToken`, and declare OpenAPI response metadata in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/RentalReturnsController.cs`
- [x] T019 [US1] Register `ReturnVehicleUseCase` using the existing composition pattern in `src/GtMotive.Estimate.Microservice.ApplicationCore/ApplicationConfiguration.cs`

**Checkpoint**: US1 is usable end to end and independently proves a valid return releases the active assignment.

---

## Phase 4: User Story 2 - Rechazar un vehÃ­culo no alquilado (Priority: P1)

**Goal**: Reject a vehicle with no active rental, a repeated return, and concurrent duplicate returns without changing history.

**Independent Test**: Exercise never-rented, already-returned, and two-concurrent-return cases; verify `409`, exactly one successful close, and one unchanged return timestamp.

### Tests for User Story 2

> Write these tests first and confirm that they fail before completing the conflict behavior.

- [x] T020 [P] [US2] Add functional cases for never-rented vehicle, repeated return, and two concurrent returns with exactly one success in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Rentals/ReturnVehicleConflictTests.cs`
- [x] T021 [P] [US2] Add Host-level `409` coverage for non-rented, repeated, and concurrent return requests in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/ReturnVehicleConflictEndpointTests.cs`

### Implementation for User Story 2

- [x] T022 [US2] Complete vehicle-not-rented, already-returned, and lost-race result handling while preserving the first `EndedAt` in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleUseCase.cs`
- [x] T023 [US2] Map all state and concurrency conflicts to stable `409` problem codes in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehiclePresenter.cs`

**Checkpoint**: US2 independently proves that only an active rental can be returned and only one concurrent request can close it.

---

## Phase 5: User Story 3 - Proteger la asignaciÃ³n de otra persona (Priority: P2)

**Goal**: Reject a return by a non-owner and distinguish invalid or missing person/vehicle references without changing the active rental.

**Independent Test**: Attempt a return with another known person, unknown references, and empty identifiers; verify `409`, `404`, and `400` respectively while the original rental remains active.

### Tests for User Story 3

> Write these tests first and confirm that they fail before completing validation and ownership behavior.

- [x] T024 [P] [US3] Add functional coverage for wrong owner, unknown person, unknown vehicle, and state preservation in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Rentals/ReturnVehicleOwnershipTests.cs`
- [x] T025 [P] [US3] Add Host-level `400`, `404`, and ownership `409` contract coverage in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/ReturnVehicleValidationEndpointTests.cs`

### Implementation for User Story 3

- [x] T026 [US3] Complete ordered person/vehicle existence and ownership validation with stable result codes in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleUseCase.cs`
- [x] T027 [US3] Complete validation, not-found, and ownership mappings without exposing internal details in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/ReturnVehiclePresenter.cs`

**Checkpoint**: All three stories work independently and invalid callers cannot alter another person's rental.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Confirm compatibility, observability, documentation, and reproducible delivery across all stories.

- [x] T028 [P] Add unit coverage for return result factories and stable error codes in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/ReturnVehicleResultTests.cs`
- [x] T029 [P] Update the repository-facing API documentation and return examples in `README.md`
- [x] T030 Verify structured logs contain outcome plus rental/vehicle identifiers but no sensitive person data in `src/GtMotive.Estimate.Microservice.ApplicationCore/Rentals/Return/ReturnVehicleUseCase.cs`
- [x] T031 Run restore, Release build, analyzers, and all three test projects using the commands in `specs/004-devolver-vehiculo/quickstart.md`
- [x] T032 Validate `docker compose config`, local startup, container startup, `POST /rentals/returns`, and a subsequent successful re-rental using `specs/004-devolver-vehiculo/quickstart.md`
- [x] T033 Verify `.dockerignore`, `Dockerfile`, `compose.yaml`, Visual Studio Docker profile, declared ports/configuration, official .NET 9 runtime images, and absence of committed or layered secrets, documenting any issue in `specs/004-devolver-vehiculo/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Starts immediately.
- **Foundational (Phase 2)**: Depends on Setup and blocks every user story.
- **US1 (Phase 3)**: Depends on Foundational and supplies the complete happy-path slice.
- **US2 (Phase 4)**: Depends on the US1 case use and endpoint, then adds state/concurrency conflicts.
- **US3 (Phase 5)**: Depends on the US1 case use and endpoint; it can proceed in parallel with US2 after US1.
- **Polish (Phase 6)**: Depends on all selected stories.

### User Story Dependency Graph

```text
Setup
  â””â”€â”€ Foundational
        â””â”€â”€ US1: valid return (MVP)
              â”œâ”€â”€ US2: no active rental / duplicate / concurrency
              â””â”€â”€ US3: ownership / invalid / not found
                    â””â”€â”€ Polish (after US2 and US3)
```

### Within Each User Story

- Write the story's tests and confirm failure before implementing behavior.
- Complete domain and port prerequisites before case-use orchestration.
- Complete the case use before presenter and endpoint integration.
- Run the story-specific unit, functional, and infrastructure filters at its checkpoint.
- Preserve prior story behavior when adding later failure paths.

### Parallel Opportunities

- T002 can run independently from the baseline test T001.
- T005 and T007 target separate layers after T004 defines the domain shape.
- T012, T013, and T014 can run in parallel after Foundational.
- T016 can run in parallel with T015.
- T020 and T021 can run in parallel.
- T024 and T025 can run in parallel; US2 and US3 phases can also run in parallel after US1.
- T028 and T029 can run in parallel before final verification.

---

## Parallel Example: User Story 1

```text
Task T012: Add the functional happy-path test in ReturnVehicleUseCaseTests.cs
Task T013: Add the Host/HTTP happy-path test in ReturnVehicleEndpointTests.cs
Task T014: Define ReturnVehicleCommand.cs and ReturnVehicleResult.cs
```

## Parallel Example: User Story 2

```text
Task T020: Add functional state, repetition, and concurrency tests
Task T021: Add Host-level 409 and concurrency tests
```

## Parallel Example: User Story 3

```text
Task T024: Add functional ownership and not-found tests
Task T025: Add Host-level 400, 404, and ownership 409 tests
```

---

## Implementation Strategy

### MVP First

1. Complete Setup.
2. Complete Foundational.
3. Complete US1.
4. Stop and run US1 unit, functional, and Host-level tests.
5. Demonstrate one active rental returning `200`, becoming `Closed`, and releasing the person and vehicle.

### Incremental Delivery

1. Setup + Foundational establish safe `Rental.Return` and atomic persistence.
2. US1 delivers the successful return path.
3. US2 hardens state transitions, repetition, and concurrency.
4. US3 adds ownership and reference protection.
5. Polish validates all quality and reproducibility gates.

### Parallel Team Strategy

1. Complete Setup and Foundational together.
2. Build US1 as the shared vertical slice.
3. After US1, implement US2 and US3 concurrently because their tests live in separate files and extend distinct error families.
4. Rejoin for complete verification and Docker validation.

## Notes

- `[P]` means different files and no dependency on unfinished work.
- `[US1]`, `[US2]`, and `[US3]` provide traceability to the specification.
- A task is complete only when its relevant tests pass and existing rental behavior remains green.
- Do not add a vehicle availability flag or a separate return document; `Rental` remains the consistency boundary.
- Commit after each task or cohesive task group if the Git hook workflow is used.
