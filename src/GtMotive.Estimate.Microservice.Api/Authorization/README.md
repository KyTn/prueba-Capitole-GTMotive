# API authorization catalog

The API authenticates bearer tokens with the authority configured in
`AppSettings:JwtAuthority`. Authorization uses exact, case-sensitive `permission`
claims and fails closed.

| Resource | Policy | Permission claim | Endpoint |
|---|---|---|---|
| `Vehicles` | `Vehicles.Create` | `permission=Vehicles.Create` | `POST /vehicles` |
| `Vehicles` | `Vehicles.Read` | `permission=Vehicles.Read` | `GET /vehicles` |
| `Rentals` | `Rentals.Create` | `permission=Rentals.Create` | `POST /rentals` |
| `Rentals` | `Rentals.Return` | `permission=Rentals.Return` | `POST /rentals/returns` |

Every declared policy is required. Repeated policy names are evaluated once.
Unauthenticated callers receive `401`; authenticated callers missing any permission
receive `403`. Tokens and claim contents must not be written to responses or logs.

`ApiAuthorizationAttribute` supplies an `ApiAuthorizationRequirement` directly to
ASP.NET Core through `IAuthorizationRequirementData`. `ApiAuthorizationHandler`
evaluates the requirement through the domain `IAuthorizationService`, so the standard
authorization middleware owns the `401`/`403` response and no MVC authorization filter
is required.
