# Feature Specification: Listar vehículos de la flota

**Feature Branch**: `002-listar-vehiculos`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "Listar los vehículos disponibles de la flota. Crear un endpoint que permita listar todos los vehículos. Crear test de infraestructura para comprobar el endpoint de lista de todos los vehículos a nivel de host. Crear test unitario para validar el método de lista de vehículos sin dependencias. Crear test funcional realizando una prueba de integración excluyendo el host."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consultar la flota disponible (Priority: P1)

Como responsable de flota, quiero consultar todos los vehículos registrados para conocer de forma completa qué vehículos están disponibles para las operaciones de la flota.

**Why this priority**: Es la capacidad principal solicitada y aporta el valor mínimo utilizable: disponer de una visión completa y actual de la flota.

**Independent Test**: Se puede probar registrando varios vehículos conocidos, solicitando el listado y comprobando que cada vehículo aparece exactamente una vez con todos sus datos públicos.

**Acceptance Scenarios**:

1. **Given** una flota con varios vehículos registrados, **When** el responsable solicita el listado, **Then** recibe todos los vehículos existentes, cada uno una sola vez y con sus datos públicos completos.
2. **Given** una flota con un único vehículo registrado, **When** el responsable solicita el listado, **Then** recibe una colección con exactamente ese vehículo.

---

### User Story 2 - Consultar una flota vacía (Priority: P2)

Como responsable de flota, quiero obtener un resultado válido cuando todavía no existen vehículos para distinguir una flota vacía de un fallo de consulta.

**Why this priority**: Evita ambigüedad en el estado inicial de la flota y permite que los consumidores procesen el resultado sin tratar la ausencia de vehículos como un error.

**Independent Test**: Se puede probar solicitando el listado sobre una flota sin vehículos y verificando que la operación finaliza correctamente con una colección vacía.

**Acceptance Scenarios**:

1. **Given** una flota sin vehículos registrados, **When** el responsable solicita el listado, **Then** recibe una colección vacía y una confirmación de consulta satisfactoria.

### Edge Cases

- Una flota vacía produce una colección vacía, no un valor nulo ni un error de elemento inexistente.
- Cada vehículo registrado aparece exactamente una vez, aunque existan vehículos con la misma marca o modelo.
- El resultado refleja los vehículos confirmados como existentes al comenzar la lectura; altas no confirmadas no deben aparecer como registros parciales.
- Si la fuente de datos no está disponible, la operación informa de un fallo general sin devolver una lista parcial como si fuera completa.
- El listado no aplica filtros por antigüedad, marca, modelo o cualquier otro atributo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir al responsable de flota solicitar el listado completo de vehículos registrados.
- **FR-002**: El sistema MUST devolver cada vehículo registrado exactamente una vez.
- **FR-003**: El sistema MUST incluir para cada vehículo su identificador, matrícula normalizada, marca, modelo y fecha de fabricación.
- **FR-004**: El sistema MUST devolver una colección vacía cuando no existan vehículos registrados.
- **FR-005**: El sistema MUST distinguir una consulta satisfactoria de un fallo inesperado al recuperar la flota.
- **FR-006**: El sistema MUST evitar presentar resultados parciales como si constituyeran el listado completo.
- **FR-007**: El contrato de consulta MUST documentar el resultado satisfactorio tanto para flotas con vehículos como vacías, y el resultado ante fallos inesperados.
- **FR-008**: La primera versión del listado MUST devolver todos los vehículos sin filtros, búsqueda ni paginación.
- **FR-009**: La consulta MUST ser de solo lectura y no modificar vehículos ni el estado de la flota.

### Key Entities

- **Vehículo**: Unidad registrada en la flota, representada por su identificador único, matrícula normalizada, marca, modelo y fecha de fabricación.
- **Flota**: Conjunto completo de vehículos registrados que el responsable desea consultar.
- **Listado de vehículos**: Colección de representaciones de vehículos; puede contener cero o más elementos y expresa el resultado completo de una consulta satisfactoria.

### Contract and Test Obligations *(mandatory)*

- **HTTP contract**: La consulta satisfactoria devuelve estado `200` y una colección de vehículos; una flota vacía devuelve el mismo estado con una colección vacía; un fallo inesperado devuelve `500` sin exponer detalles internos.
- **Unit coverage**: Una prueba pura y sin dependencias externas valida el método de listado, incluyendo el retorno de todos los vehículos conocidos y el caso de colección vacía.
- **Functional coverage**: Una prueba de integración excluyendo Host ejecuta la consulta con sus límites de aplicación y una fuente de vehículos controlada; verifica que se recuperan todos los vehículos, sin duplicados ni modificaciones.
- **Infrastructure coverage**: Una prueba a nivel de Host realiza una solicitud real al endpoint de listado y verifica el estado, el cuerpo y la correspondencia con los vehículos registrados.
- **Reproducibility**: Las tres categorías de prueba deben poder ejecutarse localmente y en el entorno de contenedores documentado sin instalar manualmente servicios externos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los vehículos registrados antes de comenzar una consulta satisfactoria aparece exactamente una vez en el resultado.
- **SC-002**: El 100 % de las consultas sobre una flota vacía finaliza satisfactoriamente con una colección vacía.
- **SC-003**: El responsable obtiene el listado completo en menos de 2 segundos en al menos el 95 % de las consultas bajo carga operativa normal.
- **SC-004**: El 100 % de los elementos devueltos contiene identificador, matrícula, marca, modelo y fecha de fabricación.
- **SC-005**: En las pruebas de aceptación, el 100 % de los usuarios distingue correctamente entre una flota vacía y un fallo de consulta.
- **SC-006**: La verificación automatizada cubre los tres niveles solicitados —método aislado, integración sin Host y recorrido completo con Host— y todos sus escenarios pasan de forma reproducible.

## Assumptions

- “Vehículos disponibles” se interpreta como todos los vehículos actualmente registrados en la flota; no existe todavía un estado separado de disponibilidad operativa, alquiler o reserva.
- El responsable que invoca la consulta ya ha sido autenticado y autorizado por los mecanismos existentes; la gestión de identidad queda fuera de esta característica.
- El volumen actual de la flota permite devolver el listado completo en una sola respuesta; filtros, ordenación configurable, búsqueda y paginación quedan fuera de esta primera versión.
- No se exige un orden concreto de los vehículos; los consumidores no deben depender del orden del resultado.
- La consulta reutiliza los datos de vehículo y la capacidad de persistencia ya definidos para la creación de vehículos.
- Crear, modificar, eliminar, alquilar o devolver vehículos queda fuera del alcance de esta característica.
