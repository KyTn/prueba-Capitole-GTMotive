# Research: Alquilar un vehículo

## Agregado y fuente de disponibilidad

- **Decision**: Modelar `Rental` como agregado y considerar disponible un vehículo cuando no existe un alquiler `Active` para su identificador.
- **Rationale**: Un único registro expresa la asociación persona–vehículo y evita mantener un indicador duplicado en `Vehicle`, que exigiría una transacción multidocumento y podría desincronizarse.
- **Alternatives considered**: Añadir `IsAvailable` a `Vehicle`, descartado por duplicar estado; actualizar vehículo y crear alquiler en una transacción, descartado por complejidad innecesaria; usar sólo el vehículo como agregado, descartado porque no representa el historial ni la futura devolución.

## Identidad canónica de persona

- **Decision**: Usar `PersonId` basado en `Guid` no vacío y consultar su existencia mediante el puerto `IPersonRegistry`.
- **Rationale**: La identidad es estable, no expone datos personales y permite que el dominio de alquiler dependa de una abstracción. El adaptador concreto puede reutilizar el registro de personas del entorno sin contaminar Domain.
- **Alternatives considered**: Nombre, correo o documento personal, descartados por mutabilidad y privacidad; aceptar cualquier texto sin validar existencia, descartado porque incumple FR-002; crear personas dentro de T3, descartado porque amplía el alcance.

## Terminología y estados

- **Decision**: “Reservar” en la restricción se interpreta como alquiler activo. T3 crea directamente `Active`; se define también `Closed` para que el modelo y los índices reconozcan alquileres finalizados, aunque la transición de devolución queda fuera de T3.
- **Rationale**: Coincide con los supuestos del spec, evita un flujo de aprobación no solicitado y hace explícita la frontera temporal de las invariantes.
- **Alternatives considered**: Estado `Pending`, descartado por no estar solicitado; eliminar registros al devolver, descartado porque perdería historial; implementar devolución ahora, descartado por alcance.

## Consistencia y concurrencia

- **Decision**: `IRentalRepository.TryAddActiveAsync` realiza una sola inserción. MongoDB usa índices únicos parciales sobre `PersonId` y `VehicleId` filtrados por `Status = Active`; el adaptador traduce la colisión a conflicto de persona o vehículo.
- **Rationale**: La inserción y la indisponibilidad son el mismo cambio observable. Los índices son la autoridad bajo carreras; las consultas previas sólo permiten mensajes tempranos.
- **Alternatives considered**: Check-then-insert sin índice, descartado por carrera; bloqueo en proceso, descartado por no funcionar con varias instancias; transacción distribuida, descartada porque el modelo de documento único no la necesita.

## Puerto de persistencia

- **Decision**: El repositorio expone lectura de alquiler activo por persona/vehículo y una creación atómica con resultado tipado (`Created`, `PersonConflict`, `VehicleConflict`).
- **Rationale**: ApplicationCore puede mapear resultados esperados sin conocer excepciones MongoDB, y los dobles de prueba reproducen la misma semántica.
- **Alternatives considered**: Exponer excepciones del driver, descartado por invertir dependencias; un método genérico `Save`, descartado porque ocultaría la obligación atómica; separar reservas por persona y vehículo, descartado por romper la consistencia.

## Contrato HTTP

- **Decision**: Exponer `POST /rentals` con `personId` y `vehicleId`; devolver `201` con `Location` y representación del alquiler, `400` para identificadores inválidos, `404` para persona/vehículo inexistente, `409` para cualquiera de las exclusividades y `500` para fallos inesperados.
- **Rationale**: Es coherente con el contrato de creación existente y distingue corrección de entrada, ausencia y estado incompatible.
- **Alternatives considered**: `POST /vehicles/{id}/rent`, descartado porque oculta que Rental es recurso propio; `422` para conflictos, descartado porque `409` describe el choque con el estado actual.

## Pruebas y dependencias

- **Decision**: Reutilizar xUnit, dobles manuales concurrentes y `WebApplicationFactory`; no incorporar paquetes.
- **Rationale**: Mantiene las tres fronteras constitucionales, reproduce carreras con `Task.WhenAll` y sigue el estilo actual.
- **Alternatives considered**: Framework de mocks, descartado por no aportar valor; MongoDB real en cada prueba Host, descartado para la matriz mínima por lentitud y dependencia; contar una prueba Host como funcional, descartado por la constitución.
