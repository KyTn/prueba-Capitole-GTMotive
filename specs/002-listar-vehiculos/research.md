# Research: Listar vehículos de la flota

## Puerto de lectura

- **Decision**: Ampliar `IVehicleRepository` con `GetAllAsync(CancellationToken)` y devolver una colección materializada de entidades `Vehicle`.
- **Rationale**: Creación y listado operan sobre el mismo agregado y almacenamiento. Un único puerto mantiene cohesionada la abstracción de persistencia, evita un repositorio redundante y permite sustituir MongoDB en pruebas.
- **Alternatives considered**: Crear `IListVehiclesRepository`, descartado por duplicar la frontera sin una necesidad diferente; devolver documentos o cursores MongoDB, descartado porque filtraría detalles técnicos hacia ApplicationCore.

## Modelo de salida compartido

- **Decision**: Mover `VehicleDto` desde `Vehicles.Create` a `ApplicationCore.Vehicles` y reutilizarlo en creación y listado.
- **Rationale**: Ambos casos de uso publican la misma representación estable —identificador, matrícula, marca, modelo y fecha de fabricación—. Compartirla evita divergencias y no introduce dependencia de transporte.
- **Alternatives considered**: Duplicar `CreateVehicleDto` y `ListVehicleDto`, descartado por añadir modelos idénticos; devolver la entidad Domain directamente desde Api, descartado porque acoplaría el contrato externo al modelo de negocio.

## Semántica de colección y orden

- **Decision**: Devolver siempre una colección no nula; `[]` representa una flota vacía y el orden no forma parte del contrato.
- **Rationale**: Una colección vacía es procesable y distingue ausencia de datos de un fallo. No imponer orden evita coste y expectativas no requeridas.
- **Alternatives considered**: `404` para flota vacía, descartado porque el recurso colección sí existe; ordenar por matrícula, descartado porque el spec no lo requiere; paginar, descartado por el volumen y alcance definidos.

## Consistencia y fallos parciales

- **Decision**: El adaptador MongoDB ejecutará la lectura y materializará todos los documentos antes de devolver el control. Si lectura o mapeo falla, propagará el fallo y no entregará una colección parcial.
- **Rationale**: Satisface el requisito de no presentar resultados parciales como completos y mantiene el caso de uso simple y de solo lectura. Sólo documentos confirmados por persistencia son candidatos al resultado.
- **Alternatives considered**: Streaming HTTP, descartado porque puede publicar un prefijo antes de detectar un fallo; transacción snapshot, descartada por añadir coordinación sin una invariante de escritura ni un requisito de instantánea estricta.

## Contrato HTTP

- **Decision**: Exponer `GET /vehicles` con `200 application/json` y un array de vehículos; documentar `500 application/problem+json` para fallos inesperados.
- **Rationale**: Reutiliza el recurso colección del POST existente, sigue la semántica habitual de consulta y permite representar igual una o cero entidades.
- **Alternatives considered**: Envolver la lista en `{ items: [...] }`, descartado porque no hay metadatos de paginación; `204` para vacío, descartado porque elimina la representación uniforme esperada.

## Estrategia de pruebas

- **Decision**: Unit probará el caso de uso con stub determinista y sin infraestructura; Functional integrará caso de uso y repositorio en memoria sin Host; Infrastructure usará `WebApplicationFactory<Program>` y el repositorio controlado ya sustituido por `VehicleApiFactory`.
- **Rationale**: Cumple las tres fronteras exigidas, reutiliza los dobles existentes y hace que los fallos sean atribuibles al nivel correcto.
- **Alternatives considered**: Usar MongoDB en todas las pruebas, descartado por perder aislamiento; contar la prueba Host como funcional e infraestructura, descartado expresamente por la constitución.

## Dependencias y observabilidad

- **Decision**: No añadir paquetes. Registrar únicamente el resultado agregado de la consulta, como cantidad recuperada, sin matrículas ni datos sensibles.
- **Rationale**: Las dependencias existentes cubren consulta, DI, HTTP y pruebas. El log agregado permite diagnóstico sin exponer información de vehículos.
- **Alternatives considered**: Añadir librerías de mapeo o mocking, descartado por complejidad innecesaria; registrar el cuerpo completo, descartado por ruido y exposición de datos.
