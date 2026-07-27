# Tasks: AutorizaciÃ³n de endpoints mediante policies y resources

**Input**: Design documents from `/specs/006-api-authorization-policies/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are mandatory because the specification and constitution require distinct unit, functional-without-Host and infrastructure-at-Host evidence.

**Organization**: Tasks are grouped by user story so each increment remains independently testable. Tests in every story must be written and observed failing before its implementation tasks begin.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it targets different files and has no dependency on an incomplete task
- **[Story]**: Maps the task to US1, US2 or US3
- Every task includes an exact file path

## Phase 1: Setup (Shared Test Infrastructure)

**Purpose**: Add reusable, deterministic authorization test support without contacting a real identity provider.

- [x] T001 Create a recording implementation of the domain authorization port with configurable policy outcomes in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/TestDoubles/RecordingAuthorizationService.cs`
- [x] T002 [P] Create a test authentication handler that builds authenticated principals from `permission` header values in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/TestAuthenticationHandler.cs`
- [x] T003 Extend the shared HTTP test factory to register the deterministic authentication scheme and test `JwtAuthority` configuration in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/AuthorizationApiFactory.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the catalog, framework policy registration, port adapter and Host configuration required by every user story.

**âš ï¸ CRITICAL**: No user story implementation begins until this phase is complete.

### Foundation tests

- [x] T004 [P] Write failing unit tests for the exact resources, policies, `permission` claim mappings and ordinal matching in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/AuthorizationCatalogTests.cs`
- [x] T005 [P] Write failing unit tests proving the domain adapter forwards the same principal, resource and policy to the framework authorization service and maps success/failure to `bool` in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/AuthorizationServiceTests.cs`
- [x] T006 [P] Write failing unit tests proving every catalog policy is registered once with its exact `permission` claim requirement in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/AuthorizationOptionsExtensionsTests.cs`
- [x] T007 Write failing infrastructure composition tests for one domain authorization adapter and non-empty `AppSettings:JwtAuthority` in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/AuthorizationCompositionTests.cs`

### Foundation implementation

- [x] T008 Implement the closed resource/policy/claim catalog defined by `contracts/authorization-catalog.md` in `src/GtMotive.Estimate.Microservice.Api/Authorization/AuthorizationCatalog.cs`
- [x] T009 Implement all catalog-backed `permission` claim policies in `src/GtMotive.Estimate.Microservice.Api/Authorization/AuthorizationOptionsExtensions.cs`
- [x] T010 Implement the domain `IAuthorizationService` adapter over `Microsoft.AspNetCore.Authorization.IAuthorizationService` with explicit type aliases in `src/GtMotive.Estimate.Microservice.Api/Authorization/AuthorizationService.cs`
- [x] T011 Register the domain authorization adapter and authorization dependencies with the correct lifetime in `src/GtMotive.Estimate.Microservice.Api/ApiConfiguration.cs`
- [x] T012 Validate non-empty `AppSettings:JwtAuthority`, keep it as the JWT authority and preserve authentication-before-authorization middleware order in `src/GtMotive.Estimate.Microservice.Host/Program.cs`

**Checkpoint**: Cataloged policies resolve through the supplied domain interface and Host authentication is configured fail-closed.

---

## Phase 3: User Story 1 - Restringir todos los endpoints de negocio (Priority: P1) ðŸŽ¯ MVP

**Goal**: Require an authenticated identity with the operation-specific permission on all four business endpoints, while preserving authorized behavior.

**Independent Test**: Invoke each endpoint without identity, with an authenticated identity lacking its permission, and with the required permission; verify `401`, `403`, and the pre-existing endpoint outcome respectively, with no business execution on denial.

### Tests for User Story 1

- [x] T013 [P] [US1] Write failing infrastructure tests parameterized over all four endpoints for unauthenticated `401` and authenticated-without-permission `403` in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/ProtectedEndpointsTests.cs`
- [x] T014 [P] [US1] Write failing infrastructure tests proving all four authorized endpoint contracts remain compatible in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/AuthorizedEndpointCompatibilityTests.cs`
- [x] T015 [US1] Write failing functional tests proving denied create/rent/return requests do not invoke handlers, publish domain events or record use-case telemetry in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Authorization/AuthorizationPipelineTests.cs`

### Implementation for User Story 1

- [x] T016 [P] [US1] Replace anonymous access with the `Vehicles.Create` policy and document `401`/`403` on `POST /vehicles` in `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/VehiclesController.cs`
- [x] T017 [P] [US1] Replace anonymous access with the `Vehicles.Read` policy and document `401`/`403` on `GET /vehicles` in `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesController.cs`
- [x] T018 [P] [US1] Replace anonymous access with the `Rentals.Create` policy and document `401`/`403` on `POST /rentals` in `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentalsController.cs`
- [x] T019 [P] [US1] Replace anonymous access with the `Rentals.Return` policy and document `401`/`403` on `POST /rentals/returns` in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/RentalReturnsController.cs`
- [x] T020 [US1] Update Swagger security detection so all protected actions expose bearer security without changing request/response schemas in `src/GtMotive.Estimate.Microservice.Host/Infrastructure/Swagger/IdentityServerApiSecurityOperationFilter.cs`

**Checkpoint**: The four endpoints are no longer anonymous; unauthenticated and insufficiently authorized requests are blocked before application execution.

---

## Phase 4: User Story 2 - Declarar autorizaciÃ³n por endpoint (Priority: P2)

**Goal**: Replace single-policy framework declarations with reusable endpoint metadata carrying exactly one resource and one or more AND-combined policies.

**Independent Test**: Inspect and execute declarations with one policy, multiple policies, duplicates and invalid names; confirm immutable metadata, one evaluation per unique policy, short-circuit denial and the same resource for every call.

### Tests for User Story 2

- [x] T021 [P] [US2] Write failing unit tests for valid single/multiple-policy declarations and rejection of null, empty or whitespace resource/policies in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/ApiAuthorizationAttributeTests.cs`
- [x] T022 [P] [US2] Write unit tests for AND evaluation, catalog validation, preserved order and first-failure short-circuit in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/ApiAuthorizationHandlerTests.cs`
- [x] T023 [US2] Write failing functional tests verifying the identical principal/resource pair reaches every unique policy and the action executes only after all succeed in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Authorization/MultiPolicyAuthorizationTests.cs`

### Implementation for User Story 2

- [x] T024 [P] [US2] Implement immutable `resourceName` plus `params policyNames` metadata, validation, deduplication and `IAuthorizationRequirementData` in `src/GtMotive.Estimate.Microservice.Api/Authorization/ApiAuthorizationAttribute.cs`
- [x] T025 [US2] Implement the native requirement and fail-closed sequential AND handler in `src/GtMotive.Estimate.Microservice.Api/Authorization/ApiAuthorizationRequirement.cs` and `src/GtMotive.Estimate.Microservice.Api/Authorization/ApiAuthorizationHandler.cs`
- [x] T026 [US2] Register the native authorization handler and domain authorization dependency in `src/GtMotive.Estimate.Microservice.Api/ApiConfiguration.cs`
- [x] T027 [P] [US2] Replace the policy declaration with resource `Vehicles` and policy `Vehicles.Create` in `src/GtMotive.Estimate.Microservice.Api/Vehicles/Create/VehiclesController.cs`
- [x] T028 [P] [US2] Replace the policy declaration with resource `Vehicles` and policy `Vehicles.Read` in `src/GtMotive.Estimate.Microservice.Api/Vehicles/List/ListVehiclesController.cs`
- [x] T029 [P] [US2] Replace the policy declaration with resource `Rentals` and policy `Rentals.Create` in `src/GtMotive.Estimate.Microservice.Api/Rentals/Rent/RentalsController.cs`
- [x] T030 [P] [US2] Replace the policy declaration with resource `Rentals` and policy `Rentals.Return` in `src/GtMotive.Estimate.Microservice.Api/Rentals/Return/RentalReturnsController.cs`
- [x] T031 [US2] Make Swagger recognize `ApiAuthorizationAttribute` as bearer-protected endpoint metadata in `src/GtMotive.Estimate.Microservice.Host/Infrastructure/Swagger/IdentityServerApiSecurityOperationFilter.cs`

**Checkpoint**: Every endpoint expresses a cataloged resource and policies through one reusable declaration; multiple policies use deterministic AND semantics.

---

## Phase 5: User Story 3 - Mantener un catÃ¡logo auditable de permisos (Priority: P3)

**Goal**: Make the authorization inventory reviewable and automatically prove complete correspondence between catalog and endpoint declarations.

**Independent Test**: Compare controller metadata against the catalog and documented assignment table; every business endpoint appears exactly once and every referenced name is defined.

### Tests for User Story 3

- [x] T032 [P] [US3] Write reflection tests that fail for any business action missing exactly one authorization declaration, using unknown names or retaining anonymous access in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/EndpointAuthorizationCoverageTests.cs`
- [x] T033 [P] [US3] Write contract tests asserting the four method/route/resource/policy assignments exactly match the versioned catalog in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/AuthorizationCatalogContractTests.cs`
- [x] T034 [US3] Write an infrastructure OpenAPI test proving every cataloged operation declares bearer security plus `401` and `403` in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/AuthorizationOpenApiTests.cs`

### Implementation for User Story 3

- [x] T035 [US3] Add purposes, claim contract, resources, policies and endpoint assignment documentation beside the executable catalog in `src/GtMotive.Estimate.Microservice.Api/Authorization/README.md`
- [x] T036 [US3] Synchronize the delivered catalog and HTTP behavior with the executable names and endpoint metadata in `specs/006-api-authorization-policies/contracts/authorization-catalog.md`
- [x] T037 [US3] Synchronize status, execution-boundary and OpenAPI guarantees with runtime behavior in `specs/006-api-authorization-policies/contracts/http-authorization.md`

**Checkpoint**: The executable catalog, endpoint metadata, OpenAPI and versioned documentation agree and drift is detected automatically.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Close security, compatibility, architecture and reproducibility gates.

- [x] T038 [P] Add tests for malformed/expired/wrong-issuer credentials and absence of sensitive response details in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/InvalidCredentialTests.cs`
- [x] T039 [P] Add a concurrency test proving authorization identities and decisions are isolated per request in `test/functional/GtMotive.Estimate.Microservice.FunctionalTests/Authorization/ConcurrentAuthorizationTests.cs`
- [x] T040 Add a bounded authorization overhead measurement for the 100 ms p95 target in `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/AuthorizationPerformanceTests.cs`
- [x] T041 Verify HTTP DTO/MediatR boundaries, exactly-one-handler composition, event publication rules and non-sensitive telemetry remain green in `test/unit/GtMotive.Estimate.Microservice.UnitTests/Mediation/ControllerMediationTests.cs`
- [x] T042 Run restore, Release build/analyzers and all three test projects using the commands in `specs/006-api-authorization-policies/quickstart.md`
- [x] T043 Validate local and Docker startup, authority configuration, health exclusion, protected endpoints, official images and absence of embedded secrets using `specs/006-api-authorization-policies/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Starts immediately.
- **Foundational (Phase 2)**: Depends on Setup and blocks every story.
- **US1 (Phase 3)**: Depends on Foundation and delivers the security MVP.
- **US2 (Phase 4)**: Depends on US1's protected endpoints, then replaces their policy-only declarations with resource-aware metadata.
- **US3 (Phase 5)**: Depends on US2 so the audit targets the final declaration model.
- **Polish (Phase 6)**: Depends on all selected stories.

### User Story Dependency Graph

```text
Setup -> Foundation -> US1 (P1/MVP) -> US2 (P2) -> US3 (P3) -> Polish
```

### Within Each User Story

- Write and observe failing tests before implementation.
- Implement shared metadata and requirement before its coordinating service/handler.
- Complete endpoint integration before OpenAPI/audit assertions.
- Stop at each checkpoint and run the story-specific tests.

### Parallel Opportunities

- T002 can run alongside T001; T004-T006 can run in parallel.
- T013 and T014 can run in parallel before T015.
- T016-T019 modify separate controllers and can run in parallel.
- T021 and T022 can run in parallel; T027-T030 modify separate controllers and can run in parallel.
- T032 and T033 can run in parallel.
- T038 and T039 can run in parallel after all stories.

---

## Parallel Example: User Story 1

```text
Task T013: Parameterized 401/403 endpoint infrastructure tests
Task T014: Authorized endpoint compatibility infrastructure tests

After tests fail:
Task T016: Protect POST /vehicles
Task T017: Protect GET /vehicles
Task T018: Protect POST /rentals
Task T019: Protect POST /rentals/returns
```

## Parallel Example: User Story 2

```text
Task T021: Attribute metadata and validation unit tests
Task T022: Filter outcome and policy coordination unit tests

After T024-T026:
Task T027: Resource-aware create-vehicle declaration
Task T028: Resource-aware list-vehicles declaration
Task T029: Resource-aware rent declaration
Task T030: Resource-aware return declaration
```

## Parallel Example: User Story 3

```text
Task T032: Endpoint coverage reflection tests
Task T033: Catalog assignment contract tests
```

---

## Implementation Strategy

### MVP First

1. Complete Setup and Foundation.
2. Complete US1 using catalog-backed framework policies.
3. Run US1 unit/functional/infrastructure evidence.
4. Stop and demonstrate that all four endpoints enforce `401`/`403` and preserve authorized contracts.

### Incremental Delivery

1. **US1**: Close anonymous access across the business API.
2. **US2**: Add resource-aware reusable declarations and multiple-policy AND behavior.
3. **US3**: Add the auditable catalog and automated drift detection.
4. **Polish**: Prove edge cases, isolation, performance, architecture and reproducibility.

### Validation Gates

- No business controller contains `[AllowAnonymous]`.
- Every business action has exactly one resource and at least one cataloged policy.
- Denied requests do not reach MediatR, use cases, events or use-case telemetry.
- Authorized requests retain existing routes, bodies and domain outcomes.
- OpenAPI exposes bearer security plus `401`/`403` for all four operations.
- Restore, build/analyzers, unit, functional, infrastructure, local and Docker checks pass.

---

## Notes

- `[P]` means no shared incomplete file dependency; tasks touching `ApiConfiguration.cs`, `Program.cs` or the Swagger filter remain sequential.
- Tests are intentionally split across unit, functional-without-Host and infrastructure-at-Host categories.
- The implementation must not add packages, persist authorization decisions or contact a real identity provider from automated tests.
- Use catalog constants at endpoint call sites; do not duplicate policy/resource string literals.

