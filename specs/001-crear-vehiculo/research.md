# Research: Crear vehículo en la flota

## Fecha y límite de antigüedad

- **Decision**: Usar `DateOnly` en el dominio y recibir la fecha actual mediante `IClock` desde ApplicationCore. `Vehicle.Create` recibe explícitamente la fecha de alta. El mínimo válido es `registrationDate.AddYears(-5)`; `DateOnly.AddYears` ajusta el 29 de febrero al último día de febrero.
- **Rationale**: El negocio opera con días, no instantes ni zonas horarias. El parámetro explícito mantiene el dominio puro y hace deterministas los límites.
- **Alternatives considered**: `DateTime.UtcNow` dentro de la entidad acopla el dominio al reloj y vuelve frágiles las pruebas; calcular años por días falla con años bisiestos.

## Normalización e identidad de matrícula

- **Decision**: Introducir `RegistrationNumber`, que aplica `Trim` y mayúsculas invariantes, rechaza vacío y expone el valor canónico. No se impone un formato nacional.
- **Rationale**: Satisface la identidad definida por la especificación sin inventar restricciones geográficas.
- **Alternatives considered**: Comparación literal admite duplicados equivalentes; una expresión regular española ampliaría el alcance.

## Unicidad concurrente

- **Decision**: Consultar por matrícula en el caso de uso para ofrecer un conflicto predecible y crear además un índice MongoDB único sobre el valor normalizado. Traducir el error de clave duplicada al mismo resultado `VehicleAlreadyExists`.
- **Rationale**: La consulta mejora el flujo normal, pero sólo la restricción atómica de almacenamiento evita carreras.
- **Alternatives considered**: Sólo comprobar antes de insertar tiene condición de carrera; una transacción distribuida añade complejidad innecesaria para un documento.

## Frontera de persistencia

- **Decision**: `IVehicleRepository` pertenece a ApplicationCore y ofrece `ExistsByRegistrationNumberAsync` y `AddAsync`, ambos con `CancellationToken`. Infrastructure implementa el puerto con una colección MongoDB `vehicles`.
- **Rationale**: Mantiene MongoDB reemplazable y el caso de uso independiente de detalles técnicos.
- **Alternatives considered**: Exponer `IMongoCollection` invierte dependencias; un repositorio genérico oculta el lenguaje de dominio y añade una abstracción especulativa.

## Contrato y errores

- **Decision**: `POST /vehicles` devuelve `201` con `Location`; `400` para contrato inválido o fecha futura, `422` para antigüedad excesiva, `409` para duplicado y `500` sólo para fallos inesperados. Los errores esperados usan `ProblemDetails` con códigos estables.
- **Rationale**: Conserva el contrato aprobado en la especificación y permite a los consumidores distinguir correcciones.
- **Alternatives considered**: Devolver `400` para todo pierde semántica; excepciones técnicas en el cuerpo exponen internals.

## Estrategia de pruebas

- **Decision**: Tres proyectos: unitarias de Domain sin mocks externos; funcionales de ApplicationCore con reloj/repositorio en memoria y sin Host; infraestructura con `WebApplicationFactory`/TestHost y MongoDB efímero proporcionado por Compose o fixture automatizado.
- **Rationale**: Cada nivel demuestra una frontera distinta y cumple la constitución sin contar una prueba dos veces.
- **Alternatives considered**: Un único proyecto dificulta imponer referencias; simular MongoDB en la prueba Host no verifica el adaptador real.

## Reproducibilidad

- **Decision**: Mantener Docker multi-stage .NET 9, fijar versión de MongoDB, aislar la base de pruebas y documentar `dotnet test` y `docker compose`. Resolver el `DockerComposeProjectPath` roto antes de cerrar la implementación.
- **Rationale**: Son requisitos constitucionales y riesgos observados en el repositorio.
- **Alternatives considered**: `mongo:latest` no es reproducible; exigir MongoDB local contradice la ejecución autónoma.
