# Data Model: Autorización de endpoints

Esta feature no persiste datos ni modifica agregados de dominio. El modelo describe metadata y decisiones inmutables por solicitud.

## PolicyDefinition

Representa un permiso registrable y evaluable.

| Field | Type | Rules |
|---|---|---|
| `Name` | string | Obligatorio, único y con comparación ordinal |
| `ClaimType` | string | Obligatorio; valor catalogado `permission` |
| `ClaimValue` | string | Obligatorio; coincide exactamente con `Name` |
| `Purpose` | string | Obligatorio para auditoría |

Validaciones:

- No admite nombres vacíos, espacios iniciales/finales ni duplicados.
- Solo una definición puede existir por nombre.
- Un nombre no registrado nunca concede acceso.

## ResourceDefinition

Representa la capacidad de negocio usada como contexto de autorización.

| Field | Type | Rules |
|---|---|---|
| `Name` | string | Obligatorio, único y estable |
| `Purpose` | string | Obligatorio |

Valores iniciales:

- `Vehicles`: operaciones sobre la flota de vehículos.
- `Rentals`: operaciones sobre alquileres.

## AuthorizationDeclaration

Metadata inmutable asociada a una action o controller.

| Field | Type | Rules |
|---|---|---|
| `ResourceName` | string | Exactamente uno; debe existir en el catálogo |
| `PolicyNames` | ordered set of string | Una o más; todos deben existir; sin duplicados efectivos |

Relaciones:

- Referencia un `ResourceDefinition`.
- Referencia una o varias `PolicyDefinition`.
- Se vincula con una `EndpointAssignment`.

Validaciones:

- Rechaza resource nulo, vacío o compuesto solo por espacios.
- Rechaza lista nula/vacía y nombres de policy vacíos.
- Normaliza espacios exteriores y deduplica con comparación ordinal.
- Policies múltiples se combinan con AND.

## EndpointAssignment

Relaciona un contrato HTTP con su declaración.

| Field | Type | Rules |
|---|---|---|
| `Method` | HTTP method | Obligatorio |
| `Route` | string | Ruta canónica |
| `Declaration` | AuthorizationDeclaration | Exactamente una |

Asignaciones:

| Method | Route | Resource | Policies |
|---|---|---|---|
| POST | `/vehicles` | `Vehicles` | `Vehicles.Create` |
| GET | `/vehicles` | `Vehicles` | `Vehicles.Read` |
| POST | `/rentals` | `Rentals` | `Rentals.Create` |
| POST | `/rentals/returns` | `Rentals` | `Rentals.Return` |

## AuthorizationDecision

Resultado efímero de la evaluación para una solicitud.

| Field | Type | Rules |
|---|---|---|
| `AuthenticationState` | unauthenticated / authenticated | Proviene del principal validado |
| `EvaluatedPolicies` | ordered set of string | Solo policies únicas hasta el cortocircuito |
| `Outcome` | challenge / forbid / allow | Exactamente uno |

State transitions:

```text
Request
  ├─ principal no autenticado ───────────────> Challenge (401)
  └─ principal autenticado
       ├─ declaración inválida/desconocida ──> Forbid / fail closed
       ├─ alguna policy = false ─────────────> Forbid (403)
       └─ todas las policies = true ─────────> Allow -> action/MediatR
```

La decisión vive solo durante la solicitud. No se almacena ni se comparte entre solicitudes concurrentes.

