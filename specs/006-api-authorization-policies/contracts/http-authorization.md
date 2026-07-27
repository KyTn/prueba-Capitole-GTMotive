# HTTP Authorization Contract

## Shared behavior

Every business endpoint uses bearer authentication and the assignment in [authorization-catalog.md](authorization-catalog.md).

| Condition | Status | Action executed | Response disclosure |
|---|---:|---|---|
| Missing, malformed, expired, not-yet-valid or wrong-issuer credential | `401 Unauthorized` | No | No token/claim details |
| Authenticated principal lacks any required policy | `403 Forbidden` | No | No missing-claim details |
| Unknown/invalid policy or resource | Access denied (fail closed) | No | No internal configuration details |
| All required policies satisfied | Existing endpoint result | Yes | Existing contract |

The authorization gate runs before controller action execution. Therefore a rejection does not send a MediatR request, invoke a use case, publish a domain event or record use-case telemetry.

## Operations

### `POST /vehicles`

- Resource: `Vehicles`
- Policy: `Vehicles.Create`
- Existing authorized responses remain `201`, `400`, `409`, `422` as applicable.
- Adds documented `401` and `403`.

### `GET /vehicles`

- Resource: `Vehicles`
- Policy: `Vehicles.Read`
- Existing authorized responses remain `200` and `500` as applicable.
- Adds documented `401` and `403`.

### `POST /rentals`

- Resource: `Rentals`
- Policy: `Rentals.Create`
- Existing authorized responses remain `201`, `400`, `404`, `409`, `500` as applicable.
- Adds documented `401` and `403`.

### `POST /rentals/returns`

- Resource: `Rentals`
- Policy: `Rentals.Return`
- Existing authorized responses remain `200`, `400`, `404`, `409`, `500` as applicable.
- Adds documented `401` and `403`.

## OpenAPI obligations

- All four operations declare the bearer security scheme.
- All four operations document `401` and `403`.
- No operation is marked anonymous.
- Existing request/response schemas and domain error mappings remain unchanged.

