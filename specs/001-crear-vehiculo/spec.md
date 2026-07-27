# Feature Specification: Crear vehículo en la flota

**Feature Branch**: `001-crear-vehiculo`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "T1 - Gestionar la creación de nuevos vehículos para la flota. Crear un endpoint que permita crear nuevos vehículos. Crear test de infraestructura para comprobar el endpoint de creación de vehículo a nivel de host. Crear test unitario para validar el método de creación de vehículo sin dependencias. Crear test funcional realizando una prueba de integración excluyendo el host. Restricción: La flota no debe contener vehículos cuya fecha de fabricación sea superior a 5 años."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Incorporar un vehículo válido (Priority: P1)

Como responsable de flota, quiero registrar un vehículo con sus datos identificativos y su fecha de fabricación para que pase a formar parte de la flota y pueda utilizarse en operaciones posteriores.

**Why this priority**: Es la capacidad principal de T1 y aporta el valor mínimo utilizable: aumentar la flota respetando sus reglas de negocio.

**Independent Test**: Se puede probar enviando una solicitud de alta con datos válidos y una fecha de fabricación dentro de los últimos cinco años, y comprobando que el vehículo queda registrado y se devuelve con un identificador.

**Acceptance Scenarios**:

1. **Given** una matrícula que no pertenece a ningún vehículo de la flota y una fecha de fabricación posterior al día equivalente de hace cinco años, **When** el responsable solicita el alta con todos los datos obligatorios válidos, **Then** el sistema registra exactamente un vehículo y devuelve sus datos junto con un identificador único.
2. **Given** una matrícula que no pertenece a ningún vehículo de la flota y una fecha de fabricación exactamente igual al día equivalente de hace cinco años, **When** el responsable solicita el alta, **Then** el sistema acepta y registra el vehículo.

---

### User Story 2 - Impedir vehículos demasiado antiguos (Priority: P1)

Como responsable de flota, quiero que se rechace cualquier vehículo con más de cinco años para asegurar que la composición de la flota cumple la política de antigüedad.

**Why this priority**: La restricción es una invariante de negocio; incumplirla dejaría la flota en un estado inválido.

**Independent Test**: Se puede probar el cálculo de antigüedad sin dependencias externas, usando una fecha de referencia controlada y fechas de fabricación situadas a ambos lados del límite.

**Acceptance Scenarios**:

1. **Given** una fecha de fabricación anterior en un día al día equivalente de hace cinco años, **When** se intenta crear el vehículo, **Then** el sistema rechaza el alta por superar la antigüedad máxima y no modifica la flota.
2. **Given** un intento rechazado por antigüedad, **When** finaliza la operación, **Then** la misma matrícula puede utilizarse posteriormente en una solicitud válida porque el intento fallido no dejó un registro parcial.

---

### User Story 3 - Rechazar altas inválidas o duplicadas (Priority: P2)

Como responsable de flota, quiero recibir un resultado claro cuando los datos sean inválidos o el vehículo ya exista para poder corregir la solicitud sin generar duplicados.

**Why this priority**: Protege la calidad de los datos y hace que el alta sea segura y comprensible, aunque depende de la capacidad principal de creación.

**Independent Test**: Se puede probar enviando solicitudes con datos obligatorios ausentes, fecha futura o matrícula ya registrada y verificando que ninguna crea un nuevo vehículo.

**Acceptance Scenarios**:

1. **Given** que ya existe un vehículo con una matrícula determinada, **When** se solicita otra alta con esa misma matrícula, **Then** el sistema rechaza la solicitud como conflicto y conserva un único vehículo con esa matrícula.
2. **Given** una fecha de fabricación futura o la ausencia de un dato obligatorio, **When** se solicita el alta, **Then** el sistema informa de que la solicitud no es válida y no registra el vehículo.

### Edge Cases

- Una fecha exactamente cinco años anterior a la fecha de alta es válida; una fecha un día anterior no lo es.
- Si el día equivalente no existe en el año límite, como el 29 de febrero, se usa el último día de febrero para determinar la frontera.
- Una fecha de fabricación posterior a la fecha de alta se rechaza.
- Las matrículas se comparan tras eliminar espacios exteriores y sin distinguir mayúsculas de minúsculas, evitando duplicados equivalentes.
- Dos altas concurrentes de la misma matrícula deben producir un solo vehículo; una se acepta y la otra se informa como conflicto.
- Un fallo de validación o persistencia no debe dejar un vehículo parcial ni anunciar una creación inexistente.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir solicitar el alta de un vehículo proporcionando matrícula, marca, modelo y fecha de fabricación.
- **FR-002**: El sistema MUST asignar un identificador único al vehículo aceptado.
- **FR-003**: El sistema MUST registrar el vehículo aceptado una sola vez y devolver su identificador y datos normalizados.
- **FR-004**: El sistema MUST evaluar la antigüedad con respecto a la fecha del intento de alta obtenida de una fuente temporal controlable.
- **FR-005**: El sistema MUST aceptar un vehículo con una fecha de fabricación exactamente igual al límite de cinco años.
- **FR-006**: El sistema MUST rechazar un vehículo cuya fecha de fabricación sea anterior al límite de cinco años, sin modificar la flota.
- **FR-007**: El sistema MUST rechazar fechas de fabricación futuras, sin modificar la flota.
- **FR-008**: El sistema MUST exigir que matrícula, marca, modelo y fecha de fabricación contengan valores válidos.
- **FR-009**: El sistema MUST impedir que exista más de un vehículo con la misma matrícula normalizada, incluso ante solicitudes concurrentes.
- **FR-010**: El sistema MUST distinguir entre una solicitud inválida, una infracción de la antigüedad máxima, un duplicado y un fallo inesperado.
- **FR-011**: La operación MUST ser atómica: cualquier rechazo o fallo conserva la flota en el estado previo al intento.
- **FR-012**: El contrato de alta MUST documentar los datos obligatorios, el resultado satisfactorio y todos los resultados de error esperados.

### Key Entities

- **Vehículo**: Unidad incorporada a la flota. Tiene identificador único, matrícula normalizada, marca, modelo y fecha de fabricación.
- **Flota**: Conjunto de vehículos gestionados. Garantiza la unicidad de matrícula y que ningún vehículo dado de alta supere la antigüedad máxima permitida.
- **Solicitud de alta**: Datos aportados para incorporar un vehículo y fecha efectiva en la que se evalúa la operación.

### Domain Invariants *(mandatory when business state changes)*

- **INV-001**: En el momento del alta, ningún vehículo incorporado puede tener más de cinco años. Exactamente cinco años es válido; una fecha anterior al límite o futura provoca rechazo y no cambia la flota. Cubierta por los escenarios de las historias 1, 2 y 3.
- **INV-002**: La matrícula normalizada identifica de forma única a un vehículo dentro de la flota. Un duplicado, incluso concurrente, provoca conflicto y conserva un único registro. Cubierta por la historia 3 y los casos límite de concurrencia.
- **INV-003**: El alta se confirma de forma completa o no produce ningún cambio observable. Cubierta por las historias 2 y 3 y el caso límite de fallo de persistencia.

### Contract and Test Obligations *(mandatory)*

- **HTTP contract**: La creación satisfactoria devuelve estado `201`, la representación del vehículo creado y una referencia para consultarlo. Datos ausentes, mal formados o fecha futura devuelven `400`; un vehículo con más de cinco años devuelve `422`; una matrícula duplicada devuelve `409`; los fallos inesperados devuelven `500` sin exponer detalles internos.
- **Unit coverage**: Pruebas puras, sin dependencias externas, verifican la creación y las invariantes de antigüedad con una fecha de referencia controlada: dentro del límite, exactamente en el límite, un día fuera del límite y fecha futura.
- **Functional coverage**: Una prueba de integración excluyendo Host ejecuta el caso de uso con sus puertos y un adaptador de persistencia controlado; verifica una creación válida, el rechazo por antigüedad y la ausencia de cambios tras un rechazo.
- **Infrastructure coverage**: Una prueba a nivel de Host envía solicitudes HTTP reales al endpoint de alta y verifica el contrato `201`, al menos un error de negocio, el cuerpo de respuesta y la persistencia observable.
- **Reproducibility**: Las tres categorías de prueba deben poder ejecutarse localmente y en el entorno de contenedores documentado sin instalar manualmente servicios externos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los vehículos con más de cinco años o con fecha futura son rechazados sin incorporarse a la flota.
- **SC-002**: El 100 % de las fechas situadas exactamente en el límite de cinco años son aceptadas cuando el resto de datos es válido.
- **SC-003**: El 100 % de los intentos duplicados o concurrentes con la misma matrícula dejan como máximo un vehículo registrado.
- **SC-004**: Un responsable puede completar una alta válida y obtener confirmación en menos de 2 segundos en al menos el 95 % de las solicitudes bajo carga operativa normal.
- **SC-005**: Las pruebas automatizadas cubren los tres niveles requeridos —unitario sin dependencias, integración funcional sin Host e infraestructura con Host— y todas pasan de forma reproducible.
- **SC-006**: En una validación de aceptación, el 100 % de los resultados previstos permite distinguir una creación correcta de datos inválidos, antigüedad excesiva y matrícula duplicada.

## Assumptions

- El responsable que invoca la operación ya ha sido autenticado y autorizado por los mecanismos existentes; la gestión de identidad queda fuera de T1.
- La matrícula es el identificador natural de unicidad dentro de la flota, mientras que el sistema asigna un identificador técnico independiente.
- La antigüedad se calcula por fecha natural, no por año de fabricación: el límite es el día equivalente de hace cinco años respecto al intento de alta.
- Marca y modelo son texto obligatorio no vacío tras eliminar espacios exteriores; reglas de catálogo o formatos nacionales de matrícula quedan fuera de T1.
- La consulta, modificación, eliminación, alquiler y devolución de vehículos quedan fuera del alcance de esta característica.
- La capacidad de persistencia y la fuente temporal se consideran dependencias disponibles a través de los límites existentes del sistema.
