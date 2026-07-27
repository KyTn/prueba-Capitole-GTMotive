# Research: Integración de casos de uso, MediatR, eventos y telemetría

## Contratos proporcionados

**Decision**: Los comandos/query implementarán `IUseCaseInput`, los resultados `IUseCaseOutput` y los casos de uso `IUseCase<TInput>`. Se conservarán los métodos `ExecuteAsync` tipados y cancelables; la implementación de `IUseCase.Execute` delegará en la misma lógica.

**Rationale**: `IUseCase<TInput>` ya existe y devuelve `Task` sin resultado ni `CancellationToken`, mientras los flujos actuales necesitan ambos para HTTP y cancelación.

**Alternatives considered**: Cambiar la interfaz suministrada se descarta por romperla; output ports con presentadores con estado se descartan por complejidad de scope/concurrencia; incluir el token en comandos se descarta por mezclar control de ejecución y datos.

## Ubicación de MediatR

**Decision**: Mantener los `IRequest<TResultado>` y `IRequestHandler` en ApplicationCore. Los controladores inyectarán `IMediator` y mapearán cada DTO HTTP al comando/query correspondiente.

**Rationale**: Los mensajes forman parte del contrato de aplicación y no del transporte HTTP. Así los DTOs de Api no exponen MediatR y los handlers quedan cohesionados con sus casos de uso.

**Alternatives considered**: Hacer que las requests HTTP implementen `IRequest<T>` se descarta porque acopla transporte y aplicación; situar handlers en Api se descarta porque expone allí la coordinación de aplicación; sustituir casos de uso por handlers se descarta porque duplicaría/desplazaría lógica.

## Publicación de eventos

**Decision**: Crear `VehicleCreated`, `VehicleRented` y `VehicleReturned` en Domain y publicarlos desde el handler después de que el resultado confirme persistencia exitosa, mediante una sola llamada `IBusFactory.GetClient(event.GetType()).Send(event)`.

**Rationale**: La factoría/bus ya son puertos de Domain y la petición exige su uso desde handlers. El listado no cambia estado.

**Alternatives considered**: Entidades con I/O contaminan Domain; publicar en el caso de uso contradice la coordinación pedida; un outbox está fuera de alcance.

## Telemetría

**Decision**: Inyectar `ITelemetry` en cada handler. Registrar `UseCaseCompleted` con `operation` y `outcome`, y `UseCaseDurationMs` con `operation`. Medir con `Stopwatch`, clasificar éxito/rechazo/error/cancelación y emitir en `finally` sin IDs, payloads, secretos ni mensajes de excepción.

**Rationale**: Infrastructure ya registra `AppTelemetry` o `NoOpTelemetry`. El puerto permite dobles deterministas y evita dependencia del proveedor.

**Alternatives considered**: Instrumentar controllers duplica lógica; un pipeline behavior añade abstracción sin resolver eventos específicos; usar Application Insights directamente invierte dependencias.

## Compatibilidad y pruebas

**Decision**: Mantener los cuatro OpenAPI previos sin cambios y ampliar unit/functional/infrastructure con composición, conformidad, publicación y telemetría.

**Rationale**: T5 cambia coordinación interna. Las pruebas HTTP existentes cubren regresión y los dobles hacen observables los efectos laterales.

**Alternatives considered**: Versionar endpoints es innecesario; solo reflexión no demuestra delegación, orden, cancelación ni observabilidad.
