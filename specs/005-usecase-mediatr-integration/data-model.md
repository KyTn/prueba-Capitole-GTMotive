# Data Model: Integración de casos de uso, MediatR, eventos y telemetría

Esta feature no modifica documentos MongoDB ni estado persistido. Define mensajes efímeros alrededor de los agregados existentes.

## Application messages

- **CreateVehicleCommand**: `RegistrationNumber`, `Brand`, `Model`, `ManufactureDate`; implementa `IUseCaseInput`.
- **ListVehiclesQuery**: sin datos funcionales; implementa `IUseCaseInput`.
- **RentVehicleCommand / ReturnVehicleCommand**: `PersonId`, `VehicleId`; implementan `IUseCaseInput`.

## Application outputs

`CreateVehicleResult`, `ListVehiclesResult`, `RentVehicleResult` y `ReturnVehicleResult` implementan `IUseCaseOutput` y conservan discriminador, datos de éxito, código y detalle actuales. Éxito contiene DTO; rechazo contiene error y no DTO. Los handlers no reinterpretan invariantes.

## MediatR messages and flow

Cada operación de Api dispone de `IRequest<IActionResult>` y exactamente un handler. El token es el parámetro de `Handle` y llega a `ExecuteAsync`.

```text
HTTP → MediatR message → handler → application input → use case
                          ↓                ↑
HTTP ← presenter ← typed result ──────────┘
                    ├── domain event (solo mutación exitosa)
                    └── telemetry
```

## Domain events

- **VehicleCreated**: `VehicleId`; solo ante `CreateVehicleResultType.Created`.
- **VehicleRented**: `RentalId`, `VehicleId`, `PersonId`, `StartedAt`; solo ante `RentVehicleResultType.Created`.
- **VehicleReturned**: `RentalId`, `VehicleId`, `PersonId`, `EndedAt`; solo ante `ReturnVehicleResultType.Returned`.

Son inmutables, sin tecnología ni payload HTTP. No contienen matrícula, nombres ni secretos. Se selecciona bus por tipo concreto. Hay máximo un `Send` por éxito y ninguno por rechazo/error/cancelación previa.

## Telemetry signal

- Evento: `UseCaseCompleted`.
- Propiedades: `operation` (`CreateVehicle`, `ListVehicles`, `RentVehicle`, `ReturnVehicle`) y `outcome` (`success`, `rejected`, `error`, `cancelled`).
- Métrica: `UseCaseDurationMs`, propiedad `operation`.
- Prohibido: IDs, request/response completos, secretos y texto de excepciones.

## Ordering

```text
Received → UseCaseExecuting → Succeeded | Rejected | Failed | Cancelled
                                |
                                └─ mutación exitosa → EventSent

Every terminal state → TelemetryRecorded
```

Un fallo de bus se propaga y la telemetría registra `error`. No hay outbox ni reintento en esta feature.
