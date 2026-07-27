# Feature Specification: Integración de casos de uso, mensajería y telemetría

**Feature Branch**: `005-usecase-mediatr-integration`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "T5 - Usar interfaces proporcionadas y MediatR. Los UseCase usados en los controllers deben implementar IUseCase, sus request/command IUseCaseInput y sus response IUseCaseOutput. Los controllers deben usar MediatR. Los handlers deben usar los UseCase, lanzar eventos de dominio mediante BusFactory y la app debe usar la telemetría de Infrastructure."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ejecutar operaciones mediante el mediador (Priority: P1)

Como consumidor de la API, quiero que cada operación expuesta por un controlador siga un flujo de ejecución uniforme, para obtener el mismo resultado funcional sin que la capa de entrada dependa directamente de la lógica del caso de uso.

**Why this priority**: Es el cambio central: establece una única frontera de entrada y desacopla la recepción de solicitudes de la ejecución de negocio.

**Independent Test**: Puede probarse enviando una solicitud válida a cada operación existente y verificando que atraviesa el mediador, alcanza un manejador y conserva el contrato de respuesta previo.

**Acceptance Scenarios**:

1. **Given** una solicitud válida para una operación expuesta, **When** el controlador la recibe, **Then** la envía mediante MediatR y devuelve el resultado producido por el flujo correspondiente.
2. **Given** una solicitud inválida o una operación rechazada por una regla de negocio, **When** el flujo termina, **Then** el consumidor recibe la misma categoría de error y el mismo contrato público esperado para esa operación.
3. **Given** el registro de dependencias de la aplicación, **When** se inicia el servicio, **Then** todos los controladores expuestos pueden resolver su mediador y todos sus comandos pueden resolver exactamente un manejador.

---

### User Story 2 - Reutilizar casos de uso mediante contratos comunes (Priority: P2)

Como mantenedor, quiero que los casos de uso invocados desde la API y sus datos de entrada y salida cumplan los contratos comunes proporcionados, para poder identificarlos, sustituirlos y probarlos de manera consistente.

**Why this priority**: Evita variantes incompatibles entre operaciones y permite que los manejadores sean adaptadores delgados sobre lógica de aplicación ya definida.

**Independent Test**: Puede validarse inspeccionando y ejecutando cada flujo expuesto para comprobar que el caso de uso implementa `IUseCase<TInput>`, la entrada implementa `IUseCaseInput` y la salida implementa `IUseCaseOutput`.

**Acceptance Scenarios**:

1. **Given** cualquier caso de uso alcanzable desde un controlador, **When** se comprueban sus contratos, **Then** implementa `IUseCase<TInput>` y acepta una entrada que implementa `IUseCaseInput`.
2. **Given** cualquier resultado devuelto por esos casos de uso, **When** se comprueba su contrato, **Then** implementa `IUseCaseOutput`.
3. **Given** un manejador de una operación expuesta, **When** procesa su comando, **Then** delega la decisión de negocio en el caso de uso correspondiente y no duplica sus reglas.

---

### User Story 3 - Publicar resultados de dominio y observar operaciones (Priority: P3)

Como responsable de operación e integración, quiero que los flujos ejecutados publiquen los eventos de dominio pertinentes y generen telemetría mediante los componentes ya proporcionados, para que otros procesos puedan reaccionar y el comportamiento de la aplicación sea observable.

**Why this priority**: Completa el flujo técnico con integración externa y diagnóstico, sin alterar el resultado principal que recibe el consumidor.

**Independent Test**: Puede probarse ejecutando operaciones exitosas y fallidas con dobles observables del bus y la telemetría, verificando los eventos y señales emitidos.

**Acceptance Scenarios**:

1. **Given** una operación que completa un cambio de dominio y define un evento asociado, **When** su manejador finaliza correctamente el caso de uso, **Then** obtiene el bus mediante `BusFactory` y publica el evento una sola vez.
2. **Given** una operación rechazada antes de completar el cambio, **When** termina el flujo, **Then** no se publica un evento que afirme que el cambio se completó.
3. **Given** una operación procesada, **When** finaliza con éxito o error, **Then** la aplicación registra mediante la telemetría de Infrastructure señales suficientes para identificar la operación, su resultado y su duración sin incluir datos sensibles.

### Edge Cases

- Si un comando no tiene un manejador registrado, el arranque o una prueba de composición debe detectar el error antes de considerar desplegable la aplicación.
- Si existen varios manejadores para un mismo comando, la composición debe rechazarse o la validación automatizada debe impedir la entrega.
- Si el caso de uso falla, el manejador no debe publicar un evento de éxito; el error debe conservar el mapeo público de la operación.
- Si el cambio de dominio se completa pero el envío del evento falla, el fallo debe quedar registrado y propagarse según la política existente, sin publicar el mismo evento más de una vez dentro del intento.
- Una cancelación del consumidor debe propagarse desde el controlador, pasando por el manejador, hasta el caso de uso y sus dependencias.
- Una indisponibilidad de la telemetría no debe modificar el resultado de negocio; debe aplicarse el comportamiento alternativo ya proporcionado por Infrastructure.
- Los flujos concurrentes deben conservar las invariantes de dominio y emitir eventos únicamente por los cambios que realmente hayan sido aceptados.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Todo caso de uso invocado por un controlador MUST implementar `IUseCase<TInput>`.
- **FR-002**: Toda entrada o comando entregado a esos casos de uso MUST implementar `IUseCaseInput`.
- **FR-003**: Toda salida de esos casos de uso MUST implementar la interfaz proporcionada `IUseCaseOutput`.
- **FR-004**: Todo controlador MUST enviar sus solicitudes de aplicación mediante MediatR y MUST NOT invocar directamente un caso de uso.
- **FR-005**: Cada comando expuesto MUST tener exactamente un manejador resoluble en la composición de la aplicación.
- **FR-006**: Cada manejador MUST delegar la ejecución de negocio en el caso de uso correspondiente y MUST NOT duplicar validaciones o transiciones propias de este.
- **FR-007**: Los manejadores MUST propagar la cancelación recibida al caso de uso y a las operaciones posteriores que admitan cancelación.
- **FR-008**: Tras un cambio de dominio completado, el manejador MUST publicar el evento de dominio correspondiente mediante un bus obtenido de `BusFactory`, cuando dicho evento esté definido para la operación.
- **FR-009**: Un flujo que no complete el cambio de dominio MUST NOT publicar un evento que represente su finalización exitosa.
- **FR-010**: Cada evento de dominio MUST publicarse como máximo una vez por ejecución aceptada del manejador.
- **FR-011**: Los controladores MUST conservar los contratos públicos y los mapeos de éxito y error existentes después de adoptar el mediador.
- **FR-012**: La aplicación MUST usar la telemetría proporcionada por Infrastructure para registrar, como mínimo, identidad de la operación, resultado y duración.
- **FR-013**: La telemetría MUST cubrir ejecuciones exitosas, errores y cancelaciones, y MUST NOT contener información personal, secretos ni cargas completas de solicitudes o eventos.
- **FR-014**: La indisponibilidad o configuración desactivada del destino de telemetría MUST usar el comportamiento alternativo proporcionado sin impedir la ejecución de negocio.
- **FR-015**: La composición de la aplicación MUST registrar el mediador, los manejadores, los casos de uso, `BusFactory` y la telemetría necesarios para todos los flujos expuestos.
- **FR-016**: Los cambios MUST abarcar todas las operaciones actualmente expuestas por controladores, incluidas creación y listado de vehículos, alquiler y devolución.

### Key Entities

- **Comando de aplicación**: Representa la intención recibida por un controlador y transporta la entrada requerida por una operación.
- **Caso de uso**: Contiene la coordinación y las decisiones de aplicación para una operación; recibe una entrada contractual y produce una salida contractual.
- **Manejador**: Adapta un comando al caso de uso correspondiente y coordina las responsabilidades posteriores a una ejecución aceptada.
- **Evento de dominio**: Hecho inmutable que comunica un cambio de negocio completado y puede ser consumido por procesos externos.
- **Señal de telemetría**: Registro operacional de una ejecución, con identidad, resultado, duración y propiedades no sensibles.

### Domain Invariants *(mandatory when business state changes)*

- **INV-001**: Solo una ejecución que haya completado y persistido un cambio de dominio puede producir el evento que afirma dicho cambio; un rechazo, error o cancelación previa no puede producirlo.
- **INV-002**: Cada cambio de dominio aceptado produce como máximo un envío del evento correspondiente dentro de una ejecución del manejador.
- **INV-003**: La introducción del mediador no altera las reglas, transiciones válidas ni respuestas públicas de creación, listado, alquiler o devolución.

### Contract and Test Obligations *(mandatory)*

- **HTTP contract**: Las rutas, cuerpos, códigos de éxito y mapeos de errores existentes permanecen sin cambios; la mediación es transparente para el consumidor.
- **Unit coverage**: Verificar los contratos de casos de uso, entradas y salidas; la delegación de cada manejador; la propagación de cancelación; y las reglas de publicación/no publicación y telemetría sin dependencias externas.
- **Functional coverage**: Verificar cada comando con su manejador y caso de uso, incluidos éxito, rechazo y cancelación, usando dobles del bus y la telemetría sin arrancar Host.
- **Infrastructure coverage**: Recorrer por HTTP/Host las operaciones de vehículos y alquileres, comprobar que sus respuestas no cambian y observar los eventos y señales esperados.
- **Reproducibility**: La verificación local y en contenedores debe demostrar que la composición arranca, todas las operaciones son resolubles y no se requieren servicios ni secretos nuevos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de las operaciones expuestas por controladores atraviesa un único manejador y un caso de uso con los contratos comunes.
- **SC-002**: El 100 % de las pruebas de contrato públicas existentes conserva sus códigos y estructuras de respuesta tras el cambio.
- **SC-003**: El 100 % de los cambios de dominio aceptados con evento definido produce exactamente un evento observable, y el 0 % de los cambios rechazados produce un evento de éxito.
- **SC-004**: El 100 % de las ejecuciones probadas —éxito, error y cancelación— genera una señal de resultado y duración sin datos sensibles.
- **SC-005**: Al menos el 95 % de las solicitudes mantiene una respuesta visible en menos de 2 segundos bajo carga operativa normal.
- **SC-006**: La validación automatizada detecta el 100 % de los comandos expuestos sin manejador, con manejadores múltiples o con dependencias no resolubles antes del despliegue.

## Assumptions

- La mención `IUserCaseOutput` de la petición se interpreta como `IUseCaseOutput`, que es la interfaz proporcionada existente en el proyecto.
- El alcance comprende todos los controladores actuales: creación y listado de vehículos, alquiler y devolución.
- Los contratos HTTP actuales y las reglas de dominio existentes no cambian; esta feature modifica la coordinación y observabilidad de los flujos.
- Solo se publica un evento cuando la operación ya define un hecho de dominio significativo; las consultas sin cambio de estado no inventan eventos.
- `BusFactory` se refiere a la abstracción de factoría de bus proporcionada por Domain.
- Infrastructure ya proporciona implementaciones operativa y alternativa de telemetría; no se añade un proveedor nuevo.
- La política existente de propagación de fallos del bus se conserva; no se introduce persistencia transaccional de eventos ni reintentos duraderos en este alcance.
