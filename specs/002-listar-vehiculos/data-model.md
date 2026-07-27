# Data Model: Listar vehículos de la flota

## Vehicle

Entidad raíz existente que representa un vehículo registrado. Esta feature no crea nuevas reglas ni transiciones; sólo lee instancias válidas.

| Field | Type | Read contract |
|---|---|---|
| `Id` | `Guid` | Obligatorio, no vacío e inmutable |
| `RegistrationNumber` | `RegistrationNumber` | Obligatorio y expuesto por su valor normalizado |
| `Brand` | `string` | Obligatorio |
| `Model` | `string` | Obligatorio |
| `ManufactureDate` | `DateOnly` | Obligatorio y expresado como fecha natural |

## VehicleDto

Modelo de salida compartido por los casos de uso de creación y listado.

| Field | Type | Source |
|---|---|---|
| `Id` | `Guid` | `Vehicle.Id` |
| `RegistrationNumber` | `string` | `Vehicle.RegistrationNumber.Value` |
| `Brand` | `string` | `Vehicle.Brand` |
| `Model` | `string` | `Vehicle.Model` |
| `ManufactureDate` | `DateOnly` | `Vehicle.ManufactureDate` |

La proyección no modifica, normaliza de nuevo ni valida la entidad; sólo desacopla el contrato de salida del modelo Domain.

## ListVehiclesResult

Resultado satisfactorio inmutable del caso de uso.

| Field | Type | Rules |
|---|---|---|
| `Vehicles` | `IReadOnlyList<VehicleDto>` | No nulo; contiene cero o más elementos; no admite modificación por el consumidor |

No se define un resultado de “no encontrado”: una flota vacía se representa mediante `Vehicles.Count == 0`. Los fallos inesperados no son resultados de negocio y se traducen en el borde HTTP.

## Persistence document

Se reutiliza `VehicleDocument` en la colección `vehicles`:

| Field | Storage representation | Mapping |
|---|---|---|
| `Id` | UUID representado como string | `Vehicle.Id` |
| `RegistrationNumber` | string normalizado | `RegistrationNumber.Value` |
| `Brand` | string | `Vehicle.Brand` |
| `Model` | string | `Vehicle.Model` |
| `ManufactureDate` | date-time a medianoche | `DateOnly` |

`VehicleMapper` añade el mapeo inverso `ToDomain`. La reconstrucción usa una vía de rehidratación que conserva los datos ya validados sin volver a aplicar la regla temporal de alta, porque la antigüedad máxima sólo se evalúa al registrar el vehículo.

## Relationships and state

```text
Fleet 1 ── contains ── 0..* Vehicle

Registered Vehicle --list--> Registered Vehicle
Empty Fleet --list--> Empty read collection
Read failure --list--> No successful result
```

El listado no produce transición de estado, no cambia disponibilidad y no escribe en la colección.

## Requirement traceability

| Requirements | Model/design element | Verification |
|---|---|---|
| FR-001, FR-002, FR-004, FR-008 | `GetAllAsync` + `ListVehiclesResult` | Unit, functional, Host |
| FR-003 | `VehicleDto` | Unit mapping and Host response |
| FR-005, FR-006 | Complete materialization or exception | Functional failure test / boundary behavior |
| FR-007 | OpenAPI contract | Contract review and Host response |
| FR-009 | Read-only repository method | Functional repository state assertion |
