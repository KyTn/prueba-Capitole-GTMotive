# Implementation Plan: Devolver un vehículo

**Branch**: `004-devolver-vehiculo` | **Date**: 2026-07-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/004-devolver-vehiculo/spec.md`

## Summary

Implementar el caso de uso `ReturnVehicle` sobre el agregado `Rental` incorporado en T3. El dominio permitirá la transición única `Active` → `Closed` y registrará `EndedAt`; `ApplicationCore` validará persona y vehículo y solicitará al repositorio el cierre condicional del alquiler activo que vincula ambos identificadores. MongoDB realizará una actualización atómica filtrada por persona, vehículo y estado activo, por lo que una sola devolución concurrente podrá vencer. Al cambiar el estado, los índices únicos parciales existentes dejarán libres automáticamente a la persona y al vehículo. `Api` expondrá `POST /rentals/returns`; `Host` conservará la composición existente. Se añadirán pruebas unitarias aisladas, funcionales sin Host e infraestructura atravesando HTTP/Host.

## Technical Context

**Language/Version**: C# / .NET 9 (`global.json`: SDK 9.0.203)  
**Primary Dependencies**: ASP.NET Core, Microsoft.Extensions.DependencyInjection, MongoDB.Driver 2.19.0; sin paquetes nuevos  
**Storage**: MongoDB, colección existente `rentals`; se amplía el documento con `EndedAt` nullable y se reutilizan `vehicles` e `IPersonRegistry` para validar referencias  
**Testing**: xUnit 2.9.2, Microsoft.AspNetCore.Mvc.Testing; dobles manuales thread-safe para Functional e Infrastructure  
**Target Platform**: Servicio HTTP Linux, ejecución local con .NET 9 y contenedorizada con imágenes oficiales .NET 9  
**Project Type**: Microservicio web con proyectos Domain, ApplicationCore, Infrastructure, Api y Host  
**Performance Goals**: Confirmación o rechazo visible en menos de 2 segundos para al menos el 95 % de solicitudes bajo carga operativa normal  
**Constraints**: Solo se cierra un alquiler `Active` de la pareja persona/vehículo; transición y liberación atómicas; una sola victoria concurrente; `CancellationToken` propagado; no se incluyen inspección, daños, cargos ni pagos  
**Scale/Scope**: Un endpoint, un caso de uso, una transición de agregado, una actualización condicional sobre una colección y ampliación de tres proyectos de prueba

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Dependency direction — PASS**: `Rental.Return` y sus invariantes permanecen en Domain sin dependencias técnicas. `ReturnVehicleUseCase` y la ampliación de `IRentalRepository` viven en ApplicationCore; MongoDB implementa el cierre condicional; Api traduce HTTP y Host conserva la composición.
- **Domain invariants — PASS**: El agregado es la única autoridad sobre `Active` → `Closed`, la inmutabilidad de `EndedAt` y la coherencia temporal. El repositorio no construye una transición alternativa: persiste atómicamente el estado producido por el dominio únicamente cuando el documento sigue activo y pertenece a la pareja solicitada.
- **Use cases and contracts — PASS**: `ReturnVehicle` es una acción independiente. [openapi.yaml](contracts/openapi.yaml) documenta `200`, `400`, `404`, `409` y `500`. Los puertos asíncronos aceptan y propagan `CancellationToken`.
- **Test matrix — PASS**: Unit valida `Rental.Return` sin dependencias; Functional integra caso de uso y puertos en memoria sin Host; Infrastructure recorre `POST /rentals/returns` mediante Host. Titularidad, vehículo no alquilado, repetición y concurrencia tienen cobertura explícita.
- **Reproducibility — PASS**: Se reutilizan .NET 9, Docker/Compose, MongoDB 8.2.6, MockServer 5.15.0 y las configuraciones existentes. No se añaden servicios, paquetes ni secretos.
- **Quality and simplicity — PASS**: El mismo documento `Rental` sigue siendo la única fuente de disponibilidad. La actualización condicional evita transacciones distribuidas y abstracciones nuevas. Se reutilizan resultados, presentadores, logging, DI y factories de prueba.

### Post-design re-check

El contrato, el modelo de datos y el puerto comparten la misma definición: solo la pareja persona/vehículo con alquiler activo puede cerrarse. `EndedAt` forma parte del agregado y del documento; el filtro atómico sobre `Active` decide carreras, mientras los índices parciales existentes liberan ambas exclusividades al pasar a `Closed`. La lectura previa permite diferenciar `404` y conflictos de titularidad/estado, pero la escritura condicional conserva la garantía definitiva. No quedan `NEEDS CLARIFICATION` ni violaciones constitucionales.

## Project Structure

### Documentation (this feature)

```text
specs/004-devolver-vehiculo/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
├── checklists/
│   └── requirements.md
└── tasks.md                 # Creado posteriormente por /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── GtMotive.Estimate.Microservice.Domain/
│   └── Rentals/                          # Rental.Return y estado Closed
├── GtMotive.Estimate.Microservice.ApplicationCore/
│   └── Rentals/
│       ├── IRentalRepository.cs          # Lectura/cierre condicional
│       └── Return/                       # Comando, resultado y caso de uso
├── GtMotive.Estimate.Microservice.Infrastructure/
│   └── MongoDb/Rentals/                  # EndedAt, mapper y actualización atómica
├── GtMotive.Estimate.Microservice.Api/
│   └── Rentals/Return/                   # Request, response, presenter y controller
└── GtMotive.Estimate.Microservice.Host/
    └── Program.cs                        # Composición existente

test/
├── unit/GtMotive.Estimate.Microservice.UnitTests/Rentals/
├── functional/GtMotive.Estimate.Microservice.FunctionalTests/
│   ├── Rentals/
│   └── TestDoubles/
└── infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Rentals/
```

**Structure Decision**: Se mantiene la arquitectura hexagonal existente y se amplían carpetas cohesionadas. `Rental` sigue siendo agregado y frontera de consistencia; no se añade un indicador de disponibilidad al vehículo porque la ausencia de alquiler activo ya expresa esa condición.

## Design Sequence

1. Ampliar `Rental` y su rehidratación con `EndedAt`; implementar `Return(endedAt)` para proteger estado, unicidad y orden temporal, con pruebas unitarias.
2. Ampliar `RentalDto` y `IRentalRepository` con lectura del alquiler activo por vehículo y cierre condicional tipado por alquiler/estado.
3. Crear contratos de comando/resultado y `ReturnVehicleUseCase`: validar identificadores, comprobar persona y vehículo, localizar el alquiler activo, validar titularidad, aplicar la transición y persistirla condicionalmente.
4. Ampliar `RentalDocument` y mapper; implementar en MongoDB una actualización única filtrada por id, persona, vehículo y `Status=Active`, escribiendo `Closed` y `EndedAt` en la misma operación.
5. Adaptar los repositorios en memoria con sincronización equivalente y añadir pruebas funcionales de éxito, vehículo no alquilado, titular incorrecto, repetición y concurrencia.
6. Añadir `POST /rentals/returns`, request/response/presenter, mapeos `400`/`404`/`409`, OpenAPI y registros del caso de uso.
7. Ampliar la factory Host para sembrar alquileres activos y probar por HTTP éxito, errores, estado final y carrera de dos devoluciones.
8. Ejecutar restore, build, analizadores, las tres suites y las comprobaciones local/Compose de [quickstart.md](quickstart.md).

## Complexity Tracking

No hay violaciones constitucionales que requieran justificación.
