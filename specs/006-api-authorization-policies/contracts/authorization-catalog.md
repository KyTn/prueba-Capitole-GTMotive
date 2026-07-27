# Authorization Catalog Contract

## Claim contract

- Claim type: `permission`
- Comparison: exact, ordinal and case-sensitive
- A policy is satisfied only when the authenticated principal contains a `permission` claim whose value exactly matches the policy name.
- Tokens, complete claim collections and personal identifiers must never be returned or logged.

## Resources

| Constant | External name | Purpose |
|---|---|---|
| `Vehicles` | `Vehicles` | Fleet vehicle operations |
| `Rentals` | `Rentals` | Rental lifecycle operations |

## Policies

| Constant | External name | Required claim |
|---|---|---|
| `VehiclesCreate` | `Vehicles.Create` | `permission=Vehicles.Create` |
| `VehiclesRead` | `Vehicles.Read` | `permission=Vehicles.Read` |
| `RentalsCreate` | `Rentals.Create` | `permission=Rentals.Create` |
| `RentalsReturn` | `Rentals.Return` | `permission=Rentals.Return` |

## Endpoint assignments

| Endpoint | Resource | Required policies |
|---|---|---|
| `POST /vehicles` | `Vehicles` | `Vehicles.Create` |
| `GET /vehicles` | `Vehicles` | `Vehicles.Read` |
| `POST /rentals` | `Rentals` | `Rentals.Create` |
| `POST /rentals/returns` | `Rentals` | `Rentals.Return` |

All policies listed for an endpoint are mandatory. Duplicate names do not create additional evaluations. Names outside this catalog fail closed.

## Declaration contract

Conceptual usage:

```csharp
[ApiAuthorization(Resources.Vehicles, Policies.VehiclesCreate)]
```

Multiple policies:

```csharp
[ApiAuthorization(Resources.Vehicles, Policies.VehiclesRead, Policies.VehiclesCreate)]
```

The declaration:

- is valid on an action and may be supported on a controller;
- contains exactly one non-empty resource;
- contains at least one non-empty policy;
- exposes immutable metadata;
- uses catalog constants rather than string literals at endpoint call sites.

