# Data Model: Crear vehículo en la flota

## Vehicle

Entidad raíz que representa un vehículo incorporado.

| Field | Type | Rules |
|---|---|---|
| `Id` | `Guid` | Generado por el sistema, no vacío e inmutable |
| `RegistrationNumber` | `RegistrationNumber` | Obligatorio, normalizado y único en la flota |
| `Brand` | `string` | Obligatorio, `Trim`, no vacío |
| `Model` | `string` | Obligatorio, `Trim`, no vacío |
| `ManufactureDate` | `DateOnly` | No futura ni anterior al límite de cinco años en la fecha de alta |

### Creation

`Vehicle.Create(id, registrationNumber, brand, model, manufactureDate, registrationDate)`:

1. Valida identificador y textos.
2. Valida que `manufactureDate <= registrationDate`.
3. Calcula `oldestAllowedDate = registrationDate.AddYears(-5)`.
4. Rechaza cuando `manufactureDate < oldestAllowedDate`.
5. Devuelve la entidad completa; no existe estado parcialmente válido.

`Vehicle` no obtiene por sí mismo el reloj ni accede a persistencia.

## RegistrationNumber

Value object que constituye la identidad natural del vehículo.

- Entrada: texto de matrícula.
- Normalización: elimina espacios exteriores y convierte a mayúsculas invariantes.
- Validez: el resultado no puede ser vacío.
- Igualdad: por valor normalizado, ordinal.
- Persistencia: se guarda el valor normalizado y se aplica un índice único.

## CreateVehicleCommand

Modelo de entrada del caso de uso:

| Field | Type | Required |
|---|---|---|
| `RegistrationNumber` | `string` | Yes |
| `Brand` | `string` | Yes |
| `Model` | `string` | Yes |
| `ManufactureDate` | `DateOnly` | Yes |

La fecha de alta no procede del consumidor; ApplicationCore la obtiene de `IClock`.

## CreateVehicleResult

Unión lógica de resultados:

- `Created(VehicleDto)`
- `InvalidInput(code, detail)`
- `VehicleTooOld(code, detail)`
- `VehicleAlreadyExists(code, detail)`

Los fallos inesperados no forman parte del resultado de negocio y se gestionan en el borde HTTP.

## Persistence document

La colección `vehicles` guarda un documento por entidad con los mismos datos esenciales. `RegistrationNumber` tiene un índice único. La inserción de un documento es la frontera atómica de T1; no se actualizan otros agregados.

## State transitions

T1 sólo contempla una transición:

```text
Not registered --valid create--> Registered
Not registered --invalid/old/duplicate/failure--> Not registered
```

No se diseñan actualización, eliminación, disponibilidad ni alquiler en esta característica.

## Invariant traceability

| Invariant | Enforcement | Storage safeguard | Tests |
|---|---|---|---|
| INV-001 age/date | `Vehicle.Create` | Sólo se persisten entidades válidas | Unit boundary matrix; functional rejection; Host `422` |
| INV-002 unique registration | `RegistrationNumber` + use case | Unique index | Functional duplicate; infrastructure concurrent/duplicate |
| INV-003 atomic creation | Factory before persistence + single insert | Atomic document insert | Functional no-change; infrastructure observable persistence |
