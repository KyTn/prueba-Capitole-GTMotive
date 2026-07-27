# Mediation and side-effect contract

| Operation | MediatR message | Application input | Output | Domain event |
|---|---|---|---|---|
| CreateVehicle | API create request | `CreateVehicleCommand` | `CreateVehicleResult` | `VehicleCreated` on `Created` |
| ListVehicles | API list query | `ListVehiclesQuery` | `ListVehiclesResult` | None |
| RentVehicle | API rent request | `RentVehicleCommand` | `RentVehicleResult` | `VehicleRented` on `Created` |
| ReturnVehicle | API return request | `ReturnVehicleCommand` | `ReturnVehicleResult` | `VehicleReturned` on `Returned` |

## Handler guarantees

1. Exactamente un handler por mensaje.
2. El handler mapea transporte sin reglas de negocio.
3. Invoca el método tipado/cancelable del caso de uso con el token sin cambios.
4. El presenter existente produce `IActionResult`.
5. Solo un resultado exitoso de mutación construye evento.
6. El handler obtiene `IBus` por el tipo del evento y llama `Send` una vez.
7. Rechazos, errores y cancelaciones previas no envían evento.
8. Toda terminación intenta telemetría con operación, resultado y duración sin valores sensibles.

Los DTOs HTTP de Api y Domain permanecen libres de MediatR. Los comandos/query y handlers viven en ApplicationCore.
