<!--
Sync Impact Report
- Version change: 1.0.0 -> 1.1.0
- Modified principles:
  - I. Hexagonal Architecture and Dependency Rule -> explicit ownership of MediatR,
    HTTP DTOs, adapters, and composition
  - III. Explicit Use Cases and Contracts -> mandatory supplied interfaces and
    HTTP-to-IRequest mapping
  - VI. Quality, Observability, and Simplicity -> mandatory domain events and telemetry
- Added sections: none
- Removed sections: none
- Templates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
  - ✅ .specify/templates/checklist-template.md
- Runtime guidance:
  - ✅ README.md
  - ✅ AGENTS.md (reviewed; already points to the current feature plan)
- Deferred implementation items:
  - TODO: Repair the encoding error in the Spec Kit Git initialization hook script.
-->

# GT Motive Renting Microservice Constitution

## Core Principles

### I. Hexagonal Architecture and Dependency Rule

The solution MUST preserve inward-only dependencies. `Domain` MUST contain business rules,
entities, value objects, domain events, and secondary-port abstractions and MUST NOT depend
on frameworks, persistence, `Infrastructure`, `Api`, or `Host`. `ApplicationCore` MUST
contain use cases, application inputs/outputs, MediatR `IRequest<T>` messages, and their
handlers. It MAY depend on MediatR abstractions but MUST NOT depend on ASP.NET Core,
MongoDB, concrete adapters, `Api`, or `Host`.

`Infrastructure` MUST implement secondary adapters such as MongoDB repositories, external
person lookup, bus, telemetry, logging, and time. `Api` MUST be a transport-only inbound
adapter containing HTTP DTOs, controllers, filters, and presenters. HTTP DTOs MUST NOT
implement `IRequest<T>` and Api MUST NOT contain application handlers. `Host` MUST remain
the composition root. Any exception MUST be documented in the plan's Complexity Tracking
with scope, rationale, and a rejected simpler alternative.

### II. Domain Rules Protected by the Model

The model MUST explicitly support adding vehicles to the fleet, listing vehicles, renting
a vehicle, and returning a vehicle. The following invariants MUST be enforced in entities,
aggregates, value objects, or domain services rather than only in controllers or adapters:

- One person MUST NOT hold more than one active rental at the same time.
- A vehicle more than five years old at registration time MUST NOT enter the fleet.
- Only an available vehicle MAY be rented.
- A return MUST be a valid `Active` to `Closed` transition and MUST make the person and
  vehicle available when the rental closes.
- Changes to an active rental MUST be atomic within its consistency boundary or use an
  explicitly documented consistency mechanism.

Feature specifications MUST define temporal boundaries, canonical identity, time source,
states, and concurrency outcomes before implementation. No adapter or handler MAY bypass
these invariants.

### III. Explicit Use Cases and Contracts

Each business action MUST have an independently identifiable use case expressed in renting
domain language. Every use case reached from a controller MUST implement
`IUseCase<TInput>`, its command/query MUST implement `IUseCaseInput`, and its result MUST
implement `IUseCaseOutput`. The same command/query MUST be an ApplicationCore
`IRequest<TResult>` handled by exactly one `IRequestHandler`. A handler MUST delegate
business decisions to the use case and MUST NOT duplicate them.

REST endpoints MUST only validate and translate transport data, map the HTTP DTO to the
corresponding ApplicationCore command/query, send it through `IMediator`, and present the
typed result. Input/output contracts MUST remain independent of ASP.NET Core and MongoDB.
Expected domain failures MUST map consistently to documented HTTP responses without
exposing internals. Asynchronous boundaries MUST propagate `CancellationToken` unless a
plan documents why cancellation is meaningless. HTTP success/error contracts MUST remain
documented in OpenAPI and feature contract artifacts.

### IV. Mandatory Testing Strategy

Automated tests are a release gate and MUST cover successful and relevant failure paths
for adding, listing, renting, and returning vehicles. Every domain invariant MUST have
focused unit coverage. Every release MUST contain distinct evidence for:

- Unit tests with no external dependencies.
- Functional integration tests exercising ApplicationCore and adapters without Host.
- Infrastructure tests crossing the HTTP/Host boundary.

A test MUST NOT count in more than one category. Tests MUST NOT require services manually
installed on the developer machine. Changes to mediation MUST verify that HTTP DTOs are
not `IRequest<T>`, Api contains no handlers, and every ApplicationCore request resolves
exactly one handler. Event tests MUST prove one publication for accepted mutations and no
success event for rejection, error, or cancellation. A change is incomplete while
restore, build, analyzers, dependency checks, or mandatory tests fail.

### V. Reproducible Local Execution and Docker

The microservice MUST run locally without manually installed external dependencies; Docker
Compose MAY supply MongoDB and MockServer. The repository MUST provide a multi-stage
Dockerfile based on official .NET 9 SDK/runtime images. The runtime image MUST exclude SDK
tooling, source, credentials, and build secrets. `.dockerignore` MUST exclude unnecessary
and sensitive content. Docker MUST remain selectable through versioned Visual Studio
launch configuration.

Required ports, environment variables, health dependencies, and volumes MUST be explicit.
Documented commands MUST prove the application works locally and in a container. Secrets
MUST NOT be committed, copied into images, or persisted in image layers.

### VI. Quality, Observability, and Simplicity

Implementations MUST be small, cohesive, and consistent with established boundaries. New
abstractions, patterns, and dependencies MUST solve a demonstrated need. Names MUST use
the fleet and renting ubiquitous language. Transport validation MUST protect HTTP
contracts, while business validation MUST remain in Domain/ApplicationCore.

Application handlers for successful state-changing operations MUST publish the applicable
immutable domain event through a client obtained from `IBusFactory`. Rejected, failed, or
cancelled operations MUST NOT emit a success event, and one accepted execution MUST send
at most one event. Query handlers MUST NOT invent state-change events.

All handlers MUST record operation identity, outcome, and duration through `ITelemetry`.
Telemetry and structured logs MUST NOT include personal identifiers, request/event
payloads, secrets, or exception text. A disabled telemetry destination MUST use the
Infrastructure no-op adapter and MUST NOT change business results.

OpenAPI/Swagger MUST remain accurate. Compiler warnings, analyzers, formatting, restore,
build, tests, and dependency-boundary checks MUST be quality gates. Breaking contracts
MUST be documented, justified, and versioned.

## Technical and Business Constraints

- The source of truth is the supplied GT Motive technical exercise and approved feature
  artifacts; sample documentation MUST NOT redefine the product domain.
- The platform is .NET 9 with `Domain`, `ApplicationCore`, `Infrastructure`, `Api`, and
  `Host` projects.
- MediatR 10.0.1 messages and handlers belong to ApplicationCore; Api HTTP DTOs remain
  separate and controllers map them explicitly before calling `IMediator`.
- Persistence MUST be accessed through ports. MongoDB is a replaceable adapter.
- `IBusFactory`, `IBus`, and `ITelemetry` are Domain ports; concrete implementations and
  environment-specific registration belong to Infrastructure.
- Vehicle age, vehicle availability, one-active-rental-per-person, ownership, and valid
  return transition are mandatory invariants.
- Local/container execution MUST require no manual external-service installation.
- Container builds MUST use official .NET images and MUST NOT require committed secrets.
- Functional requirements belong in feature specs; this constitution governs how they
  are designed, implemented, and verified.

## Development Workflow and Quality Gates

Every feature MUST follow this sequence:

1. Specify independently testable acceptance criteria, failure paths, and boundaries.
2. Resolve material ambiguities and pass Constitution Check before and after design.
3. Design domain model, consistency boundaries, ports, application messages, and HTTP
   contracts before adapters.
4. Define unit, functional, and infrastructure tests with requirement traceability.
5. Implement core behavior, handlers, adapters, and composition without reversing
   dependencies.
6. Verify HTTP DTOs do not implement `IRequest<T>`, Api has no handlers, and every
   ApplicationCore request resolves exactly one handler.
7. Verify successful mutations publish once, failures publish none, and all handler
   outcomes produce non-sensitive telemetry.
8. Run restore, build, analyzers, unit, functional, and infrastructure tests.
9. Validate documented startup locally and with Docker.
10. Review dependency direction and ensure rules are neither duplicated nor bypassed.
11. Document decisions, justified deviations, and remaining risks.

Reviews MUST reject changes when a mandatory gate lacks evidence. Plans MUST record every
constitutional violation in Complexity Tracking with its correction plan.

## Governance

This constitution supersedes incompatible practices and sample documentation. Every spec,
plan, task list, implementation review, and release decision MUST verify compliance.
Exceptions require written approval, bounded scope, impact analysis, and a dated
correction plan; they do not silently amend this constitution.

Amendments MUST update this file, its Sync Impact Report, and every affected template or
guidance document. Versioning follows semantic versioning: MAJOR for incompatible
governance changes or principle removal/redefinition, MINOR for a new principle or
materially expanded obligations, and PATCH for non-semantic clarification. Compliance
audits occur during planning, after design, during code review, and before delivery.

**Version**: 1.1.0 | **Ratified**: 2026-07-27 | **Last Amended**: 2026-07-27
