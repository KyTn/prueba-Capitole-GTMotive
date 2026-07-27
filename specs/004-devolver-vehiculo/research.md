# Research: Devolver un vehículo

## Decisión 1: Frontera de consistencia

**Decision**: Mantener `Rental` como única fuente de verdad de la asignación y derivar la disponibilidad de la ausencia de un alquiler activo.

**Rationale**: T3 ya protege un alquiler activo por persona y vehículo mediante índices únicos parciales. Cambiar el mismo documento de `Active` a `Closed` elimina al alquiler de ambos índices y libera las dos exclusividades como un único cambio observable.

**Alternatives considered**:

- Añadir `IsAvailable` a `Vehicle`: exigiría coordinar dos documentos y abriría estados contradictorios.
- Crear un documento separado de devolución: duplicaría estado y requeriría transacciones o reconciliación.

## Decisión 2: Transición del dominio

**Decision**: Incorporar `Rental.Return(DateTimeOffset endedAt)` como transición única de `Active` a `Closed`; debe rechazar un alquiler ya cerrado y un instante anterior a `StartedAt`.

**Rationale**: La constitución exige que la regla viva en el modelo. El método puro permite verificar estado, temporalidad e inmutabilidad sin infraestructura.

**Alternatives considered**:

- Asignar el estado desde el caso de uso: permitiría omitir invariantes desde otros adaptadores.
- Tratar una segunda devolución como éxito idempotente: contradice la especificación, que exige rechazar la repetición.

## Decisión 3: Identificación de la devolución

**Decision**: El comando contiene `PersonId` y `VehicleId`; el alquiler candidato es el alquiler activo actual del vehículo y debe pertenecer a la persona indicada.

**Rationale**: Sigue literalmente la relación de T4, protege la asignación de otra persona y no obliga al consumidor a conocer el identificador interno del alquiler.

**Alternatives considered**:

- Recibir únicamente `RentalId`: no acredita la pareja persona/vehículo requerida por la especificación.
- Recibir únicamente `VehicleId`: permitiría cerrar el alquiler de otro titular.

## Decisión 4: Persistencia y concurrencia

**Decision**: Persistir mediante una actualización MongoDB de un solo documento filtrada por `Id`, `PersonId`, `VehicleId` y `Status=Active`, estableciendo conjuntamente `Status=Closed` y `EndedAt`. El puerto devuelve `Closed` o `Conflict`.

**Rationale**: La operación de documento único es atómica. En dos devoluciones concurrentes, solo la primera encuentra el estado activo; la segunda obtiene conflicto sin sobrescribir `EndedAt`.

**Alternatives considered**:

- Leer y luego reemplazar sin condición de estado: vulnerable a lost updates y dobles confirmaciones.
- Usar una transacción: innecesaria para un solo documento y añade complejidad operativa.
- Confiar solo en la mutación en memoria: no protege carreras entre instancias del servicio.

## Decisión 5: Diferenciación de errores

**Decision**: El caso de uso valida en orden entrada, existencia de persona, existencia de vehículo, alquiler activo y titularidad. La actualización condicional es la autoridad final y cualquier pérdida de carrera se traduce a conflicto.

**Rationale**: Las lecturas aportan errores útiles (`400`, `404`, `409`), pero no se consideran garantía de concurrencia. El filtro de escritura evita que una observación obsoleta confirme una devolución inválida.

**Alternatives considered**:

- Una única actualización sin lecturas: sería consistente, pero no distinguiría recursos inexistentes de vehículo no alquilado o titular incorrecto.
- Devolver `404` para ausencia de alquiler activo: la especificación define ese caso como conflicto de estado.

## Decisión 6: Contrato HTTP

**Decision**: Exponer `POST /rentals/returns`, con cuerpo `{ personId, vehicleId }`; devolver `200` con el alquiler cerrado, `400` para entrada inválida, `404` para persona/vehículo inexistente, `409` para estado o titularidad incompatibles y `500` para fallo inesperado.

**Rationale**: La devolución es un comando de negocio no idempotente en su contrato —la repetición falla— y `POST` representa la ejecución del comando sin confundirla con un reemplazo completo del alquiler.

**Alternatives considered**:

- `PATCH /rentals/{id}`: expone una edición genérica de estado y exige un identificador no incluido en la intención del usuario.
- `DELETE /rentals/{id}`: borraría semánticamente el historial que debe conservarse.

## Decisión 7: Compatibilidad de datos y pruebas

**Decision**: Añadir `EndedAt` nullable. Los documentos activos existentes sin el campo siguen siendo válidos; los cerrados se rehidratan con su fecha de fin. Actualizar dobles existentes para simular el compare-and-set y reutilizar la factory Host.

**Rationale**: El cambio es aditivo para los datos de T3 y no requiere migración destructiva. Dobles con la misma semántica permiten cubrir los tres límites sin servicios instalados manualmente.

**Alternatives considered**:

- Migración obligatoria de todos los documentos: no aporta valor para alquileres activos, cuyo `EndedAt` debe ser nulo.
- Mocking framework nuevo: innecesario; el repositorio ya emplea dobles manuales.
