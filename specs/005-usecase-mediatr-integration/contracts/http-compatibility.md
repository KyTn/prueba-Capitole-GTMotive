# HTTP compatibility contract

T5 no añade ni modifica endpoints:

| Operación | Método y ruta | Éxito | Contrato canónico |
|---|---|---|---|
| Crear vehículo | `POST /vehicles` | `201` + `Location` | [001 OpenAPI](../../001-crear-vehiculo/contracts/openapi.yaml) |
| Listar vehículos | `GET /vehicles` | `200` + array | [002 OpenAPI](../../002-listar-vehiculos/contracts/openapi.yaml) |
| Alquilar vehículo | `POST /rentals` | `201` + `Location` | [003 OpenAPI](../../003-alquilar-vehiculo/contracts/openapi.yaml) |
| Devolver vehículo | `POST /rentals/returns` | `200` | [004 OpenAPI](../../004-devolver-vehiculo/contracts/openapi.yaml) |

Se conservan campos, validación, cuerpos, headers, códigos `2xx`, errores `400`/`404`/`409`/`422`/`500`, `application/problem+json` y `code`. MediatR, eventos y telemetría no aparecen en HTTP ni añaden headers.
