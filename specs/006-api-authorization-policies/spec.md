# Feature Specification: Autorización de endpoints mediante policies y resources

**Feature Branch**: `006-api-authorization-policies`  
**Created**: 2026-07-27  
**Status**: Draft  
**Input**: User description: "Crear AuthorizationService que use el JwtAuthority definido en el appsettings. Crear documento de definicion de policies y de Resources. Crear un Attribute que permita aplicar uno o varios policyName y un resource name, que se pueda aplicar a cada endpoint donde se necesite. Aplicar autorización a todos los endpoints de la capa Api tal y como define la interfaz IAuthorizationService."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Restringir todos los endpoints de negocio (Priority: P1)

Como responsable de seguridad, quiero que cada endpoint de negocio exija una identidad válida y los permisos declarados para su operación y recurso, para impedir accesos no autorizados.

**Why this priority**: Es el objetivo de seguridad principal y elimina la exposición anónima actual de las operaciones de vehículos y alquileres.

**Independent Test**: Se puede invocar cada endpoint sin identidad, con una identidad sin permisos y con una identidad autorizada, verificando respectivamente rechazo por autenticación, rechazo por permisos y ejecución normal.

**Acceptance Scenarios**:

1. **Given** una solicitud sin identidad autenticada, **When** se invoca cualquiera de los cuatro endpoints de negocio, **Then** la solicitud se rechaza antes de ejecutar la operación.
2. **Given** una identidad autenticada que no satisface al menos una policy requerida, **When** invoca el endpoint, **Then** recibe una denegación y la operación de negocio no se ejecuta.
3. **Given** una identidad autenticada que satisface todas las policies requeridas para el recurso declarado, **When** invoca el endpoint, **Then** la autorización permite continuar y se conserva el contrato funcional previo del endpoint.

---

### User Story 2 - Declarar autorización por endpoint (Priority: P2)

Como desarrollador de la API, quiero declarar en cada endpoint uno o varios nombres de policy y un nombre de recurso, para que sus requisitos de acceso sean visibles, consistentes y reutilizables.

**Why this priority**: Una declaración uniforme reduce omisiones y permite revisar la cobertura de autorización sin inspeccionar la lógica interna de cada operación.

**Independent Test**: Se puede inspeccionar la metadata de cada endpoint y comprobar que contiene exactamente un recurso, una o más policies conocidas y ninguna excepción anónima.

**Acceptance Scenarios**:

1. **Given** un endpoint que requiere una sola policy, **When** se consulta su declaración de autorización, **Then** se obtiene esa policy y un único nombre de recurso no vacío.
2. **Given** un endpoint que requiere varias policies, **When** se autoriza una solicitud, **Then** todas las policies declaradas se evalúan contra la misma identidad y recurso, y basta un fallo para denegar el acceso.
3. **Given** una declaración con una policy o recurso desconocidos, **When** se valida o ejecuta la autorización, **Then** el acceso se deniega de forma segura y la configuración inválida queda identificable.

---

### User Story 3 - Mantener un catálogo auditable de permisos (Priority: P3)

Como mantenedor, quiero un documento único con las policies, los resources y su asignación a endpoints, para revisar y evolucionar el modelo de autorización sin ambigüedad.

**Why this priority**: El catálogo permite auditar la cobertura y evita que los nombres usados por la API se separen de su definición.

**Independent Test**: Se puede comparar automáticamente el catálogo con la metadata de los endpoints y comprobar que todos los nombres declarados existen y que todos los endpoints de negocio están cubiertos.

**Acceptance Scenarios**:

1. **Given** el catálogo de autorización, **When** se revisa una policy o resource, **Then** se encuentra su nombre estable, propósito y endpoints asociados.
2. **Given** los cuatro endpoints de negocio existentes, **When** se comparan con el catálogo, **Then** cada uno aparece exactamente una vez con su resource y sus policies requeridas.

### Edge Cases

- Un token ausente, malformado, expirado, todavía no válido o emitido por una autoridad distinta se trata como identidad no autenticada.
- Una identidad autenticada sin los claims necesarios, o con claims incompletos, no satisface la policy afectada.
- Si se declaran varias policies, se aplican con semántica acumulativa: todas deben cumplirse.
- Una policy duplicada en la misma declaración se evalúa como un único requisito y no altera el resultado.
- Una declaración sin policies, con nombres vacíos o con resource vacío es inválida y nunca concede acceso.
- Una policy o resource no registrado produce denegación segura; no se sustituye por un permiso general.
- Un fallo o indisponibilidad durante la validación de identidad o evaluación de permisos no permite ejecutar la operación.
- Las solicitudes concurrentes se autorizan de forma independiente y no comparten identidad ni resultado de autorización.
- El endpoint de salud queda fuera de la capa API de negocio y de este alcance.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST autenticar las solicitudes de la API de negocio contra la autoridad de identidad configurada para el entorno.
- **FR-002**: El sistema MUST rechazar identidades cuyo emisor no coincida con la autoridad configurada, así como credenciales ausentes, inválidas o fuera de vigencia.
- **FR-003**: El sistema MUST ofrecer una única capacidad de autorización que determine si una identidad satisface una policy para un recurso, conforme al contrato `IAuthorizationService`.
- **FR-004**: El sistema MUST permitir que cada endpoint declare uno o varios nombres de policy y exactamente un nombre de resource mediante metadata reutilizable.
- **FR-005**: El sistema MUST exigir que todas las policies declaradas se satisfagan para conceder acceso cuando un endpoint declare más de una.
- **FR-006**: El sistema MUST evaluar todas las policies de un endpoint para la misma identidad autenticada y el mismo resource declarado.
- **FR-007**: El sistema MUST denegar el acceso si una policy o resource declarado es vacío, desconocido, inválido o no puede evaluarse.
- **FR-008**: El sistema MUST impedir la ejecución de la operación de negocio cuando falle la autenticación o cualquier evaluación de autorización.
- **FR-009**: El sistema MUST proteger los endpoints `POST /vehicles`, `GET /vehicles`, `POST /rentals` y `POST /rentals/returns`; ninguno MUST permitir acceso anónimo.
- **FR-010**: El sistema MUST conservar, para solicitudes autorizadas, las rutas, entradas, salidas y códigos funcionales existentes.
- **FR-011**: El sistema MUST responder a solicitudes sin autenticación con el resultado estándar de autenticación requerida y a identidades autenticadas sin permisos con el resultado estándar de acceso prohibido.
- **FR-012**: El sistema MUST proporcionar un catálogo versionado que defina cada policy, cada resource y la asignación de ambos a cada endpoint protegido.
- **FR-013**: Los nombres usados en las declaraciones de endpoints MUST corresponder exactamente con entradas del catálogo.
- **FR-014**: El sistema MUST permitir comprobar que todo endpoint de negocio presente o futuro tiene una declaración de autorización válida, salvo que una excepción anónima esté expresamente documentada y aprobada.
- **FR-015**: Los resultados de autenticación o autorización MUST NOT revelar credenciales, claims sensibles ni detalles internos de evaluación.

### Key Entities

- **Policy**: Regla de acceso con nombre estable y propósito documentado que una identidad debe satisfacer.
- **Resource**: Nombre estable del objeto o capacidad de negocio sobre el que se evalúa una policy.
- **Declaración de autorización**: Asociación de un endpoint con exactamente un resource y una o varias policies; todas las policies son obligatorias.
- **Identidad autenticada**: Representación de la persona o sistema llamante, validada por la autoridad configurada y portadora de los atributos necesarios para evaluar policies.
- **Entrada de catálogo**: Definición auditable que relaciona endpoint, operación, resource y policies.

### Contract and Test Obligations *(mandatory)*

- **HTTP contract**: Los cuatro contratos HTTP de negocio permanecen iguales para accesos autorizados; se añaden respuestas de autenticación requerida y acceso prohibido, sin cuerpo que exponga información sensible.
- **Application contract**: La decisión de acceso conserva la firma conceptual de `IAuthorizationService`: identidad, resource y un nombre de policy producen una decisión booleana; la coordinación de múltiples policies invoca ese contrato para cada policy y exige que todas sean favorables.
- **Events and telemetry**: Una denegación no genera eventos de dominio ni ejecuta telemetría propia del caso de uso; los registros de seguridad, si existen, no contienen tokens ni claims sensibles.
- **Unit coverage**: Verificar declaraciones con una y varias policies, semántica acumulativa, deduplicación, entradas inválidas, correspondencia con catálogo y adaptación exacta al contrato de autorización.
- **Functional coverage**: Verificar autorización favorable, policy incumplida, policy/resource desconocidos y que una denegación impide enviar la operación al caso de uso.
- **Infrastructure coverage**: Recorrer los cuatro endpoints con credencial ausente, inválida, sin permisos y autorizada; verificar autoridad configurada, códigos de rechazo, cobertura completa y contratos de éxito sin cambios.
- **Reproducibility**: Las comprobaciones deben poder ejecutarse localmente y en el entorno contenedorizado usando valores de configuración no secretos y credenciales de prueba controladas.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los cuatro endpoints de negocio rechaza solicitudes no autenticadas antes de ejecutar su operación.
- **SC-002**: El 100 % de los endpoints de negocio declara al menos una policy válida y exactamente un resource incluido en el catálogo.
- **SC-003**: El 100 % de las pruebas con una identidad que incumple al menos una policy resulta en denegación, incluso cuando satisface las restantes.
- **SC-004**: El 100 % de las solicitudes con identidad y permisos válidos conserva el código y contenido funcional esperado antes de introducir autorización.
- **SC-005**: Una revisión automatizada detecta el 100 % de los nombres de policy/resource no catalogados y de los endpoints de negocio sin cobertura.
- **SC-006**: Al menos el 95 % de las decisiones de autenticación y autorización se completa sin añadir más de 100 ms al tiempo percibido de la solicitud bajo carga normal.
- **SC-007**: Cero credenciales, tokens o claims sensibles aparecen en respuestas de denegación o evidencias de prueba.

## Assumptions

- `AppSettings:JwtAuthority` continúa siendo la fuente de configuración de la autoridad y proporciona un valor distinto por entorno.
- La autoridad existente emite los claims necesarios; la gestión de usuarios, emisión de tokens y administración del proveedor de identidad quedan fuera de alcance.
- Varias policies en un endpoint tienen semántica AND: todas deben cumplirse.
- El resource declarado es un nombre estable catalogado y se entrega como contexto de evaluación; no implica cargar una entidad de persistencia salvo que una policy futura lo requiera.
- Los cuatro endpoints MVC actuales constituyen toda la capa API de negocio dentro del alcance; `/health/live` y la documentación interactiva no son endpoints de negocio.
- El catálogo de policies y resources es la fuente de verdad mantenida junto al código y debe evolucionar en el mismo cambio que cualquier nueva declaración.

