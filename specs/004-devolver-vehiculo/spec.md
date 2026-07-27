# Feature Specification: Devolver un vehículo

**Feature Branch**: `004-devolver-vehiculo`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "T4 - Devolver un vehículo. Crear un endpoint que permita devolver un vehículo previamente alquilado por una persona. Crear test de infraestructura para comprobar el endpoint de devolver un vehículo a nivel de Host. Crear test unitario para validar el método de devolver un vehículo sin dependencias. Crear test funcional realizando una prueba de integración excluyendo el Host. Restricción: el vehículo a devolver debe estar previamente alquilado."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Devolver un vehículo alquilado (Priority: P1)

Como persona que mantiene un alquiler activo, quiero devolver el vehículo alquilado para cerrar mi asignación y dejar el vehículo disponible para futuros alquileres.

**Why this priority**: Es la capacidad central de T4 y completa el ciclo iniciado por el alquiler.

**Independent Test**: Se puede probar partiendo de un alquiler activo entre una persona y un vehículo, solicitando la devolución y comprobando que el alquiler queda finalizado una sola vez y que el vehículo deja de estar asociado a un alquiler activo.

**Acceptance Scenarios**:

1. **Given** una persona y un vehículo vinculados por un alquiler activo, **When** esa persona devuelve el vehículo, **Then** el sistema finaliza el alquiler, registra el momento de devolución y confirma los identificadores del alquiler, la persona y el vehículo.
2. **Given** una devolución confirmada, **When** se consulta el estado de la asignación, **Then** no existe un alquiler activo para ese vehículo ni para esa persona.

---

### User Story 2 - Rechazar la devolución de un vehículo no alquilado (Priority: P1)

Como responsable de flota, quiero impedir la devolución de un vehículo que no tenga un alquiler activo para conservar un historial coherente y evitar cambios inexistentes.

**Why this priority**: Es la restricción explícita de T4 y protege la transición de estado del alquiler.

**Independent Test**: Se puede probar solicitando la devolución de un vehículo registrado sin alquiler activo y verificando que la operación se rechaza sin modificar su estado.

**Acceptance Scenarios**:

1. **Given** un vehículo registrado sin alquiler activo, **When** se solicita su devolución, **Then** el sistema rechaza la operación como conflicto y no crea ni finaliza ningún alquiler.
2. **Given** un vehículo cuyo alquiler ya fue finalizado, **When** se repite la devolución, **Then** el sistema la rechaza como conflicto y conserva el momento de devolución original.

---

### User Story 3 - Proteger la asignación de otra persona (Priority: P2)

Como persona usuaria, quiero que solo pueda devolverse el vehículo asociado a mi alquiler activo para que nadie cierre por error la asignación de otra persona.

**Why this priority**: Evita alteraciones indebidas y asegura que la devolución identifica de forma inequívoca la asignación que debe finalizar.

**Independent Test**: Se puede probar intentando devolver, con una persona distinta, un vehículo alquilado y comprobando que el alquiler original permanece activo.

**Acceptance Scenarios**:

1. **Given** un vehículo alquilado activamente por una persona, **When** otra persona solicita devolverlo, **Then** el sistema rechaza la operación como conflicto y mantiene intacto el alquiler activo.
2. **Given** identificadores ausentes, mal formados o correspondientes a recursos inexistentes, **When** se solicita la devolución, **Then** el sistema devuelve el error correspondiente y no cambia ningún alquiler.

### Edge Cases

- Dos devoluciones concurrentes del mismo alquiler producen exactamente una finalización confirmada; la otra se rechaza sin sobrescribir el momento de devolución.
- La repetición de una devolución ya confirmada no modifica el historial ni se considera una nueva devolución.
- Un vehículo registrado pero nunca alquilado no puede devolverse.
- Una persona distinta de la titular no puede devolver el vehículo mediante esta operación.
- Un fallo durante la finalización conserva activo el alquiler y no deja el vehículo disponible parcialmente.
- La devolución se aplica al alquiler activo vigente; los alquileres finalizados anteriores no son candidatos.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir solicitar la devolución mediante los identificadores de la persona y del vehículo.
- **FR-002**: El sistema MUST comprobar que la persona y el vehículo existen antes de confirmar la devolución.
- **FR-003**: El sistema MUST localizar un alquiler activo que vincule exactamente a la persona y al vehículo indicados.
- **FR-004**: El sistema MUST rechazar la devolución cuando el vehículo no tenga un alquiler activo.
- **FR-005**: El sistema MUST rechazar la devolución cuando el alquiler activo del vehículo pertenezca a otra persona.
- **FR-006**: Una devolución válida MUST finalizar el alquiler activo y registrar un único momento de devolución.
- **FR-007**: Tras una devolución válida, el vehículo y la persona MUST dejar de estar vinculados por un alquiler activo.
- **FR-008**: El sistema MUST impedir que dos solicitudes concurrentes finalicen más de una vez el mismo alquiler.
- **FR-009**: El sistema MUST rechazar identificadores ausentes o mal formados sin modificar el estado de los alquileres.
- **FR-010**: El sistema MUST distinguir entre solicitud inválida, persona o vehículo inexistente, conflicto de estado o titularidad y fallo inesperado.
- **FR-011**: La operación MUST ser atómica: la finalización y la liberación de la asignación se hacen observables conjuntamente o no se produce ningún cambio.
- **FR-012**: El contrato de devolución MUST documentar los datos obligatorios, la confirmación satisfactoria y todos los resultados de error esperados.

### Key Entities

- **Persona**: Titular identificada del alquiler activo que solicita la devolución.
- **Vehículo**: Unidad registrada que solo puede devolverse mientras esté vinculada a un alquiler activo.
- **Alquiler**: Asignación entre una persona y un vehículo, con estado, momento de inicio y, al finalizar, un único momento de devolución.

### Domain Invariants *(mandatory when business state changes)*

- **INV-001**: Solo un alquiler activo puede transicionar a finalizado; devolver un vehículo sin alquiler activo provoca conflicto y no cambia el historial. Cubierta por la historia 2.
- **INV-002**: La persona que solicita la devolución y el vehículo deben coincidir con ambos extremos del alquiler activo; una discrepancia provoca conflicto y conserva el alquiler. Cubierta por la historia 3.
- **INV-003**: Un alquiler se finaliza como máximo una vez y su momento de devolución no puede sobrescribirse, incluso ante solicitudes concurrentes. Cubierta por la historia 2 y los casos límite de concurrencia.
- **INV-004**: Finalizar el alquiler y liberar la asignación activa constituyen un único cambio atómico; ante rechazo o fallo, el alquiler permanece activo y el vehículo continúa no disponible. Cubierta por las historias 1 a 3.

### Contract and Test Obligations *(mandatory)*

- **HTTP contract**: La devolución satisfactoria devuelve estado `200` y la representación del alquiler finalizado. Identificadores ausentes o mal formados devuelven `400`; una persona o vehículo inexistente devuelve `404`; un vehículo sin alquiler activo, una devolución repetida o una persona distinta de la titular devuelve `409`; los fallos inesperados devuelven `500` sin exponer detalles internos.
- **Unit coverage**: Pruebas puras, sin dependencias externas, validan que el método de devolución finaliza un alquiler activo una sola vez, registra la devolución y rechaza finalizar un alquiler que ya no está activo sin alterar su estado.
- **Functional coverage**: Una prueba de integración excluyendo Host ejecuta el caso de uso con sus límites de aplicación y adaptadores controlados; verifica devolución válida, vehículo no alquilado, titular incorrecto y conservación del estado tras un rechazo.
- **Infrastructure coverage**: Una prueba a nivel de Host envía solicitudes HTTP reales al endpoint de devolución y verifica el contrato `200`, los errores `400`, `404` y `409`, los cuerpos de respuesta y el estado observable tras la operación.
- **Reproducibility**: Las tres categorías de prueba deben poder ejecutarse localmente y en el entorno de contenedores documentado sin instalar manualmente servicios externos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de las devoluciones válidas finaliza exactamente el alquiler activo correspondiente y elimina la asignación activa de la persona y del vehículo.
- **SC-002**: El 100 % de los intentos sobre vehículos sin alquiler activo se rechaza sin modificar el historial.
- **SC-003**: El 100 % de los intentos de una persona distinta de la titular se rechaza y conserva intacto el alquiler activo.
- **SC-004**: Ante devoluciones concurrentes del mismo vehículo, exactamente una se confirma y el momento de devolución se registra una sola vez.
- **SC-005**: La persona recibe confirmación o un motivo esperado de rechazo en menos de 2 segundos en al menos el 95 % de las solicitudes bajo carga operativa normal.
- **SC-006**: En una validación de aceptación, el 100 % de los resultados permite distinguir una devolución confirmada, datos inválidos, recursos inexistentes y conflictos de devolución.
- **SC-007**: La verificación automatizada cubre los tres niveles solicitados —método aislado, integración sin Host y recorrido HTTP completo con Host— y todos sus escenarios pasan de forma reproducible.

## Assumptions

- La devolución identifica tanto a la persona como al vehículo, siguiendo la relación creada al alquilar, para impedir que una persona cierre el alquiler de otra.
- “Previamente alquilado” significa que el vehículo posee un alquiler activo en el momento exacto de procesar la devolución; un alquiler histórico ya finalizado no satisface la restricción.
- La devolución es efectiva inmediatamente y no contempla inspección, daños, cargos, pagos, kilometraje ni aprobación manual.
- El momento de devolución usa la referencia temporal del sistema y, una vez registrado, no puede modificarse mediante esta operación.
- Finalizar el alquiler hace que la persona pueda volver a alquilar y que el vehículo pueda volver a ser alquilado; crear un nuevo alquiler queda fuera de T4.
- La consulta de personas, vehículos y alquileres activos se considera disponible a través de los límites existentes del sistema.
