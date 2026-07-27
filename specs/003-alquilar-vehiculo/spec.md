# Feature Specification: Alquilar un vehículo

**Feature Branch**: `003-alquilar-vehiculo`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "T3 - Alquilar un vehículo. Crear un endpoint que permita alquilar un vehículo por una persona. Crear test de infraestructura para comprobar el endpoint de alquilar un vehículo a nivel de Host. Crear test unitario para validar el método de alquilar un vehículo sin dependencias. Crear test funcional realizando una prueba de integración excluyendo el Host. Restricción: una misma persona no debería poder reservar más de un vehículo al mismo tiempo."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Alquilar un vehículo disponible (Priority: P1)

Como persona usuaria de la flota, quiero alquilar un vehículo disponible para que quede asignado a mí y no pueda ser alquilado simultáneamente por otra persona.

**Why this priority**: Es la capacidad central de T3 y aporta el valor mínimo utilizable: asociar de forma exclusiva una persona y un vehículo.

**Independent Test**: Se puede probar partiendo de una persona sin alquiler activo y un vehículo disponible, solicitando el alquiler y comprobando que se confirma una única asignación activa con ambos identificadores.

**Acceptance Scenarios**:

1. **Given** una persona identificada sin alquiler activo y un vehículo registrado y disponible, **When** solicita alquilarlo, **Then** el sistema crea exactamente un alquiler activo, confirma la asignación y devuelve los identificadores del alquiler, la persona y el vehículo.
2. **Given** un alquiler confirmado, **When** otra persona intenta alquilar el mismo vehículo mientras continúa activo, **Then** el sistema rechaza el intento como conflicto y conserva el alquiler original.

---

### User Story 2 - Impedir varios alquileres por persona (Priority: P1)

Como responsable de flota, quiero impedir que una persona mantenga más de un vehículo alquilado al mismo tiempo para cumplir la política de asignación individual.

**Why this priority**: Es la restricción de negocio explícita de T3; sin ella, la operación permitiría estados inválidos.

**Independent Test**: Se puede probar sin dependencias externas aplicando la regla a una persona que ya tiene un alquiler activo y comprobando que un segundo intento se rechaza sin alterar ninguna asignación.

**Acceptance Scenarios**:

1. **Given** una persona con un alquiler activo y otro vehículo disponible, **When** intenta alquilar el segundo vehículo, **Then** el sistema rechaza la solicitud como conflicto y mantiene sin cambios el alquiler existente y la disponibilidad del segundo vehículo.
2. **Given** dos solicitudes concurrentes de la misma persona para vehículos diferentes y ningún alquiler activo previo, **When** ambas se procesan, **Then** como máximo una se confirma y la persona queda asociada a un solo vehículo.

---

### User Story 3 - Rechazar referencias inválidas (Priority: P2)

Como persona usuaria, quiero recibir un resultado claro cuando la persona o el vehículo indicados no sean válidos para poder corregir la solicitud sin producir asignaciones parciales.

**Why this priority**: Protege la integridad de la flota y hace comprensibles los fallos, aunque depende del flujo principal de alquiler.

**Independent Test**: Se puede probar solicitando alquileres con identificadores ausentes, una persona inexistente o un vehículo inexistente y verificando que no se crea ningún alquiler.

**Acceptance Scenarios**:

1. **Given** un identificador de persona o vehículo ausente o mal formado, **When** se solicita el alquiler, **Then** el sistema informa de que la solicitud no es válida y no modifica ningún estado.
2. **Given** una persona o un vehículo que no existe, **When** se solicita el alquiler, **Then** el sistema informa de que el recurso no se encuentra y no crea un alquiler.

### Edge Cases

- Si la persona ya alquila el mismo vehículo, repetir la solicitud se rechaza como conflicto y no crea un alquiler duplicado.
- Dos personas que intentan alquilar concurrentemente el mismo vehículo producen como máximo un alquiler confirmado.
- Dos solicitudes concurrentes de una persona para vehículos distintos producen como máximo un alquiler confirmado.
- Una persona o un vehículo inexistente no genera registros parciales.
- Un fallo al confirmar la asignación conserva tanto a la persona como al vehículo en su estado anterior.
- La regla se aplica a alquileres activos; los alquileres finalizados no bloquean un alquiler posterior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir solicitar el alquiler de un vehículo mediante los identificadores de una persona y de un vehículo.
- **FR-002**: El sistema MUST comprobar que la persona y el vehículo existen antes de confirmar el alquiler.
- **FR-003**: El sistema MUST comprobar que el vehículo está disponible antes de confirmar el alquiler.
- **FR-004**: El sistema MUST comprobar que la persona no tiene otro alquiler activo antes de confirmar el alquiler.
- **FR-005**: El sistema MUST crear un único alquiler activo para una solicitud válida y devolver sus identificadores de alquiler, persona y vehículo.
- **FR-006**: El sistema MUST impedir que un vehículo tenga más de un alquiler activo, incluso ante solicitudes concurrentes.
- **FR-007**: El sistema MUST impedir que una persona tenga más de un alquiler activo, incluso ante solicitudes concurrentes.
- **FR-008**: El sistema MUST rechazar identificadores ausentes o mal formados sin modificar el estado de la flota.
- **FR-009**: El sistema MUST distinguir entre solicitud inválida, persona o vehículo inexistente, conflicto de disponibilidad y fallo inesperado.
- **FR-010**: La operación MUST ser atómica: un rechazo o fallo conserva sin cambios los alquileres y la disponibilidad de los vehículos.
- **FR-011**: El contrato de alquiler MUST documentar los datos obligatorios, la confirmación satisfactoria y todos los resultados de error esperados.
- **FR-012**: La finalización o devolución del alquiler MUST quedar fuera del alcance de T3; esta característica únicamente crea alquileres activos.

### Key Entities

- **Persona**: Individuo identificado que solicita un vehículo y puede mantener como máximo un alquiler activo.
- **Vehículo**: Unidad registrada en la flota que puede estar disponible o asociada a un único alquiler activo.
- **Alquiler**: Asignación exclusiva y activa entre una persona y un vehículo, con identificador único y momento de inicio.

### Domain Invariants *(mandatory when business state changes)*

- **INV-001**: Una persona puede estar asociada como máximo a un alquiler activo en cualquier instante. Un segundo intento, incluido uno concurrente, provoca conflicto y no modifica el alquiler existente ni el vehículo solicitado. Cubierta por la historia 2 y los casos límite de concurrencia.
- **INV-002**: Un vehículo puede estar asociado como máximo a un alquiler activo en cualquier instante. Un intento sobre un vehículo ya alquilado, incluido uno concurrente, provoca conflicto y conserva la asignación original. Cubierta por la historia 1 y los casos límite de concurrencia.
- **INV-003**: Un alquiler activo sólo puede relacionar una persona existente y un vehículo existente. Una referencia inexistente provoca rechazo y no cambia el estado. Cubierta por la historia 3.
- **INV-004**: La confirmación del alquiler y la indisponibilidad del vehículo constituyen un único cambio atómico; ambos se hacen observables o ninguno. Cubierta por las historias 1 a 3 y el caso límite de fallo de confirmación.

### Contract and Test Obligations *(mandatory)*

- **HTTP contract**: La creación satisfactoria del alquiler devuelve estado `201`, la representación de la asignación y una referencia al alquiler. Identificadores ausentes o mal formados devuelven `400`; una persona o vehículo inexistente devuelve `404`; una persona con alquiler activo o un vehículo no disponible devuelve `409`; los fallos inesperados devuelven `500` sin exponer detalles internos.
- **Unit coverage**: Pruebas puras y sin dependencias externas validan el método y las invariantes de alquiler: aceptación cuando persona y vehículo están libres, rechazo de una persona con alquiler activo y rechazo de un vehículo con alquiler activo, sin cambios parciales.
- **Functional coverage**: Una prueba de integración excluyendo Host ejecuta el caso de uso con sus límites de aplicación y adaptadores controlados; verifica un alquiler válido, el conflicto por segundo vehículo de la misma persona y la conservación del estado tras el rechazo.
- **Infrastructure coverage**: Una prueba a nivel de Host envía solicitudes HTTP reales al endpoint de alquiler y verifica el contrato `201`, el conflicto `409` al intentar un segundo alquiler para la misma persona, los cuerpos de respuesta y el estado observable.
- **Reproducibility**: Las tres categorías de prueba deben poder ejecutarse localmente y en el entorno de contenedores documentado sin instalar manualmente servicios externos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de las solicitudes válidas crea exactamente un alquiler activo que vincula la persona y el vehículo solicitados.
- **SC-002**: El 100 % de los intentos, incluidos los concurrentes, deja como máximo un alquiler activo por persona.
- **SC-003**: El 100 % de los intentos, incluidos los concurrentes, deja como máximo un alquiler activo por vehículo.
- **SC-004**: El 100 % de las solicitudes rechazadas conserva sin cambios los alquileres existentes y la disponibilidad de los demás vehículos.
- **SC-005**: Una persona recibe la confirmación o el motivo esperado de rechazo en menos de 2 segundos en al menos el 95 % de las solicitudes bajo carga operativa normal.
- **SC-006**: En una validación de aceptación, el 100 % de los resultados permite distinguir una confirmación, datos inválidos, recursos inexistentes y conflictos de alquiler.
- **SC-007**: La verificación automatizada cubre los tres niveles solicitados —método aislado, integración sin Host y recorrido HTTP completo con Host— y todos sus escenarios pasan de forma reproducible.

## Assumptions

- La persona se identifica mediante un identificador estable ya conocido por el sistema; el alta y la autenticación de personas quedan fuera de T3.
- “Alquilar” crea inmediatamente una asignación activa; no existe un estado previo de reserva pendiente ni aprobación manual.
- La restricción expresada como “reservar” se aplica al alquiler activo: una persona no puede mantener simultáneamente más de una asignación.
- Un vehículo sólo se considera disponible cuando no tiene un alquiler activo.
- La fecha y hora de inicio se registran usando la referencia temporal del sistema; precios, pagos, duración prevista, seguros y condiciones contractuales quedan fuera de T3.
- La devolución o finalización de un alquiler será una capacidad posterior; se asume que alquileres ya finalizados no cuentan para la restricción, aunque T3 no implementa esa transición.
- La capacidad de consultar personas, vehículos y alquileres se considera disponible a través de los límites del sistema.
