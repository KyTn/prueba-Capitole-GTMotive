# Data Model: Alquilar un vehículo

## PersonId

Valor canónico que identifica a la persona sin almacenar datos personales.

| Field | Type | Rules |
|---|---|---|
| Value | Guid | Obligatorio; distinto de vacío |

La existencia se valida mediante `IPersonRegistry`; el valor no implica que T3 gestione el ciclo de vida de la persona.

## Rental

Agregado que representa la asignación exclusiva de un vehículo a una persona.

| Field | Type | Rules |
|---|---|---|
| Id | Guid | Obligatorio, único e inmutable |
| PersonId | PersonId | Persona existente; inmutable durante el alquiler |
| VehicleId | Guid | Vehículo existente; inmutable durante el alquiler |
| StartedAt | DateTimeOffset | Obligatorio; obtenido de una fuente temporal controlable |
| Status | RentalStatus | `Active` al crear; `Closed` reservado para una devolución posterior |
| EndedAt | DateTimeOffset? | Nulo mientras está activo; no se modifica en T3 |

### Creation

`Rental.Create(id, personId, vehicleId, startedAt)` rechaza identificadores vacíos y crea el agregado en `Active`, con `EndedAt = null`.

### State transitions

```text
[none] ── RentVehicle ──> Active
Active ── ReturnVehicle ──> Closed   # Fuera del alcance de T3
```

T3 sólo implementa la primera transición. No existen estados `Pending` o `Reserved`.

## Vehicle

Entidad existente reutilizada sin añadir un indicador de disponibilidad.

| Relevant field | Type | Rental rule |
|---|---|---|
| Id | Guid | Debe existir antes de alquilar |

La disponibilidad se proyecta como `no active Rental where VehicleId = vehicle.Id`.

## Relationships

```text
Person identity 1 ── 0..* Rental * ── 1 Vehicle
                       │
                       └─ como máximo 1 Active por PersonId
Vehicle            ────── como máximo 1 Active por VehicleId
```

Los alquileres cerrados pueden acumularse para una misma persona o vehículo; las restricciones sólo cubren `Active`.

## Persistence model

`RentalDocument` refleja los campos del agregado. La colección `rentals` contiene:

- índice único por `Id`;
- índice único parcial por `PersonId` cuando `Status == Active`;
- índice único parcial por `VehicleId` cuando `Status == Active`.

La inserción de un documento activo constituye el cambio atómico. No se actualiza `Vehicle`; por tanto, una inserción rechazada no deja estado parcial.

## Validation and failure mapping

| Condition | Domain/application result | HTTP |
|---|---|---|
| `PersonId` o `VehicleId` vacío/mal formado | InvalidInput | 400 |
| Persona no encontrada | PersonNotFound | 404 |
| Vehículo no encontrado | VehicleNotFound | 404 |
| Alquiler activo para persona | PersonAlreadyHasActiveRental | 409 |
| Alquiler activo para vehículo | VehicleNotAvailable | 409 |
| Inserción satisfactoria | Created | 201 |

## Invariant traceability

| Invariant | Primary protection | Concurrent protection | Test boundary |
|---|---|---|---|
| INV-001: un activo por persona | `RentVehicleUseCase` + repository port | Unique partial PersonId index | Unit result; Functional + Host concurrency |
| INV-002: un activo por vehículo | `RentVehicleUseCase` + repository port | Unique partial VehicleId index | Unit result; Functional + Host concurrency |
| INV-003: referencias existentes | Person/vehicle lookup ports | Checked before insertion | Functional + Host 404 |
| INV-004: atomicidad | Single `Rental` document | Atomic insert + indexes | Functional state assertion + Host observable state |
