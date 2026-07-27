<!--
Sync Impact Report
- Version change: template (unversioned) -> 1.0.0
- Modified principles:
  - Template placeholders -> I. Hexagonal Architecture and Dependency Rule
  - Template placeholders -> II. Domain Rules Protected by the Model
  - Template placeholders -> III. Explicit Use Cases and Contracts
  - Template placeholders -> IV. Mandatory Testing Strategy
  - Template placeholders -> V. Reproducible Local Execution and Docker
  - Added VI. Quality, Observability, and Simplicity
- Added sections:
  - Technical and Business Constraints
  - Development Workflow and Quality Gates
- Removed sections: none
- Templates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
  - ✅ .specify/templates/checklist-template.md (reviewed; no change required)
- Runtime guidance:
  - ⚠ README.md remains pending because it describes the obsolete Virtual Wallet domain.
- Deferred implementation items:
  - TODO: Define the exact five-year boundary, person identity, and rental states in a feature spec.
  - TODO: Implement the fleet/rental domain and the three required test categories.
  - TODO: Add .dockerignore, a versioned Visual Studio Docker profile, and the referenced
    docker-compose.dcproj (or remove that reference).
  - TODO: Make container restore/build independent of a private feed and build-time PAT.
  - TODO: Repair the encoding error in the Spec Kit Git initialization hook script.
-->

# GT Motive Renting Microservice Constitution

## Core Principles

### I. Hexagonal Architecture and Dependency Rule
The solution MUST preserve inward-only dependencies. `Domain` MUST contain the business
model and rules and MUST NOT depend on frameworks, persistence, `Infrastructure`, `Api`,
or `Host`. `ApplicationCore` MUST orchestrate use cases through domain types and ports and
MUST NOT depend on concrete adapters. `Infrastructure` MUST implement secondary adapters.
`Api` MUST be an inbound adapter, while `Host` MUST be the composition root. MongoDB,
ASP.NET Core, Docker, authentication, logging, and other technologies MUST NOT contaminate
the domain model. Any exception MUST be documented in the plan's Complexity Tracking table
with its scope, rationale, and a rejected simpler alternative. This rule keeps business
behavior independently testable and replaceable.

### II. Domain Rules Protected by the Model
The model MUST explicitly support adding vehicles to the fleet, listing available vehicles,
renting a vehicle, and returning a vehicle. The following invariants MUST be enforced in
entities, aggregates, value objects, or domain services rather than only in controllers or
adapters:

- One person MUST NOT hold more than one active rental or reservation at the same time.
- A vehicle more than five years old at registration time MUST NOT enter the fleet.
- Only an available vehicle MAY be rented.
- A return MUST be a valid state transition and MUST make the vehicle available when the
  rental closes.
- Changes to a rental and its vehicle MUST be atomic within the consistency boundary or
  use an explicitly documented consistency mechanism.

The feature specification MUST resolve the exact five-year boundary, canonical person
identity, time source, rental/reservation terminology, and allowed rental states before
implementation. No adapter MAY bypass these invariants. Central protection prevents
different entry points from producing contradictory fleet state.

### III. Explicit Use Cases and Contracts
Each business action MUST have an independently identifiable use case expressed in renting
domain language. REST endpoints MUST only validate and translate transport data, invoke an
input port, and map results; they MUST NOT implement business rules. Input/output ports
MUST remain independent of ASP.NET Core, MongoDB, and other technical details. Expected
domain failures MUST map consistently to documented HTTP responses without exposing
internals. Asynchronous boundaries SHOULD accept and propagate `CancellationToken`; any
omission MUST be justified where cancellation cannot be meaningful. HTTP success and error
contracts MUST be documented in OpenAPI and in the feature contract artifacts. Explicit
contracts make behavior testable without the delivery mechanism.

### IV. Mandatory Testing Strategy
Automated tests are a release gate and MUST cover both successful and relevant failure
paths for adding, listing, renting, and returning vehicles. Every domain invariant MUST
have focused unit coverage. At minimum, the solution MUST include three distinct tests:

- An infrastructure test exercising at least one REST method at Host level.
- A unit test validating a method or domain rule with no external dependencies.
- A functional integration test exercising application behavior while excluding Host.

A single test MUST NOT be counted in more than one category. Unit tests isolate the subject
and use no infrastructure; functional tests integrate the application and adapters below
Host; infrastructure tests cross the HTTP/Host boundary. Tests MUST NOT require services
manually installed on the developer machine. A change is incomplete while restore, build,
static analysis, or any mandatory test fails. A defect SHOULD first be reproduced by a
failing automated test when practical. This matrix demonstrates isolation as well as
end-to-end composition.

### V. Reproducible Local Execution and Docker
The microservice MUST run locally without manually installed external dependencies; Docker
or Docker Compose MAY supply them. The repository MUST provide a multi-stage Dockerfile
based on official, .NET 9-compatible `mcr.microsoft.com/dotnet/sdk` and
`mcr.microsoft.com/dotnet/aspnet` images. The runtime image MUST exclude SDK tooling,
source, credentials, and build secrets. A `.dockerignore` MUST exclude unnecessary and
sensitive content. Docker MUST be selectable as a Visual Studio startup option through
versioned project configuration. Required ports, environment variables, configuration,
health dependencies, and volumes MUST be explicit. Documented commands MUST prove the
application works both inside and outside a container. Secrets MUST NOT be committed,
copied into images, or persisted in image layers. This ensures reviewers can reproduce
the delivered system from a clean machine.

### VI. Quality, Observability, and Simplicity
Implementations MUST be small, cohesive, and consistent with the established project
boundaries. New abstractions, patterns, and dependencies MUST solve a demonstrated need;
speculative complexity is prohibited. Names MUST use the fleet and renting ubiquitous
language. Boundary validation MUST protect transport contracts, while business validation
MUST remain in the domain. Operations SHOULD emit structured logs sufficient to diagnose
use-case outcomes and MUST NOT include secrets or sensitive personal data. OpenAPI/Swagger
documentation MUST remain accurate. Compiler warnings, configured analyzers, formatting,
build, and tests MUST be treated as quality gates. Breaking contract changes MUST be
documented, justified, and versioned. These constraints preserve clarity and operability
without weakening architectural isolation.

## Technical and Business Constraints

- The functional source of truth is the supplied `PruebaTecnica NET Capitole GT Motive
  2026 (1).pdf`; conflicting sample documentation MUST NOT redefine the product domain.
- The product domain is vehicle fleet and rental management, not the README's sample
  Virtual Wallet.
- The observed platform is .NET 9 with projects `Domain`, `ApplicationCore`,
  `Infrastructure`, `Api`, and `Host`.
- Persistence MUST be accessed through ports. MongoDB is a replaceable adapter, never a
  domain dependency.
- Vehicle age, vehicle availability, and one-active-rental-per-person are mandatory
  invariants.
- Local and containerized execution MUST require no manual external-service installation.
- Container builds MUST use official .NET images and MUST NOT require committed secrets.
- Functional requirements belong in feature specifications; this constitution governs
  the non-negotiable manner in which they are designed, implemented, and verified.

## Development Workflow and Quality Gates

Every feature MUST follow this sequence:

1. Specify independently testable acceptance criteria, failure paths, and boundary cases.
2. Resolve material ambiguities and pass the plan's Constitution Check before research;
   repeat the check after design.
3. Design the domain model, consistency boundaries, ports, and contracts before adapters.
4. Define the required unit, functional, and infrastructure tests with traceability to
   requirements and invariants.
5. Implement the core behavior, adapters, and composition without reversing dependencies.
6. Run restore, build, analyzers, unit tests, functional tests, and infrastructure tests.
7. Validate documented startup both locally and with Docker.
8. Review dependency direction and ensure business rules are neither duplicated nor
   bypassed in adapters.
9. Document decisions, justified deviations, and remaining risks.

Reviews MUST reject a change when a mandatory gate lacks evidence. Plans MUST record any
constitutional violation in Complexity Tracking, including its correction plan.

## Governance

This constitution supersedes incompatible practices and sample documentation. Every
specification, plan, task list, implementation review, and release decision MUST verify
compliance. Exceptions require written approval, a bounded scope, impact analysis, and a
dated correction plan; they do not silently amend the constitution.

Amendments MUST update this file, its Sync Impact Report, and every affected template or
guidance document. Versioning follows semantic versioning: MAJOR for incompatible
governance changes or removal/redefinition of principles, MINOR for new principles or
materially expanded obligations, and PATCH for non-semantic clarification. Every review
MUST use the current ratified version. Compliance audits occur at planning, after design,
during code review, and before delivery.

**Version**: 1.0.0 | **Ratified**: 2026-07-27 | **Last Amended**: 2026-07-27
