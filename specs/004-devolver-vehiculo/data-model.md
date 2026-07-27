# Data Model: Devolver un vehículo

## Rental

Agregado que representa la asignación histórica de una persona a un vehículo y constituye la frontera de consistencia de la devolución.

### Fields

| Field | Type | Required | Rules |
|---|---|---:|---|
| `Id` | UUID | Yes | No vacío e inmutable |
| `PersonId` | UUID value object | Yes | No vacío e inmutable; identidad canónica del titular |
| `VehicleId` | UUID | Yes | No vacío e inmutable |
| `StartedAt` | Instant | Yes | Inmutable |
| `Status` | `Active` or `Closed` | Yes | Solo admite la transición `Active` → `Closed` |
| `EndedAt` | Nullable instant | Conditionally | Nulo en `Active`; obligatorio en `Closed`; no anterior a `StartedAt`; se asigna una sola vez |

### Relationships

- Cada alquiler referencia exactamente una persona.
- Cada alquiler referencia exactamente un vehículo.
- Como máximo un alquiler `Active` puede compartir `PersonId`.
- Como máximo un alquiler `Active` puede compartir `VehicleId`.
- Los alquileres `Closed` permanecen como historial y no bloquean nuevos alquileres.

### State transitions

```text
Create
  └── Active (EndedAt = null)
        └── Return(endedAt >= StartedAt)
              └── Closed (EndedAt = endedAt)
```

No existe transición desde `Closed`. Un segundo intento produce error de dominio y conserva el primer `EndedAt`.

## Person

Referencia externa usada para validar que el solicitante existe y coincide con el titular del alquiler. T4 no almacena ni modifica datos personales.

## Vehicle

Entidad existente de flota. T4 verifica su existencia, pero no añade un campo mutable de disponibilidad. El vehículo está alquilado si existe un `Rental` activo con su `VehicleId`; al cerrar ese alquiler queda disponible.

## Persistence representation

La colección `rentals` conserva los campos existentes y añade:

| Field | BSON representation | Compatibility |
|---|---|---|
| `EndedAt` | UTC datetime nullable/omitted | Los documentos activos de T3 sin el campo se interpretan con valor nulo |

El cierre aplica una única actualización condicional:

```text
match:
  Id == rental.Id
  PersonId == command.PersonId
  VehicleId == command.VehicleId
  Status == Active

set atomically:
  Status = Closed
  EndedAt = clock.UtcNow
```

Un resultado sin modificación significa conflicto concurrente o estado ya cerrado. Los índices parciales `ux_rentals_active_person` y `ux_rentals_active_vehicle` dejan de incluir el documento al cambiar a `Closed`.

## Validation and error mapping

| Condition | Domain/application outcome | HTTP |
|---|---|---:|
| Identificador vacío o mal formado | Invalid input | 400 |
| Persona inexistente | Person not found | 404 |
| Vehículo inexistente | Vehicle not found | 404 |
| Vehículo sin alquiler activo | Vehicle not rented | 409 |
| Alquiler activo de otra persona | Rental ownership conflict | 409 |
| Alquiler cerrado durante una carrera | Rental already returned/conflict | 409 |
| Transición válida persistida | Returned rental | 200 |
