# Research: Autorización de endpoints mediante policies y resources

## Autenticación frente a autorización

- **Decision**: `AppSettings:JwtAuthority` permanece en Host como fuente de la validación JWT. `AuthorizationService` recibe el `ClaimsPrincipal` ya autenticado y delega la evaluación de resource/policy al motor de autorización.
- **Rationale**: La autoridad valida emisor, firma y vigencia una sola vez en el middleware. Revalidar el token desde el servicio duplicaría red/trabajo criptográfico, mezclaría autenticación con autorización y dificultaría los tests.
- **Alternatives considered**: Validar manualmente el JWT dentro de cada llamada `Authorize` (duplicación y riesgo); consultar remotamente la autoridad por policy (latencia y dependencia no requerida); mover autenticación a cada controller (repetición y posibilidad de omisión).

## Ubicación del adaptador

- **Decision**: Implementar el port `GtMotive...Domain.Interfaces.IAuthorizationService` dentro de `Api/Authorization`, usando alias explícito para distinguirlo de `Microsoft.AspNetCore.Authorization.IAuthorizationService`.
- **Rationale**: La decisión es una puerta del transporte HTTP basada en `HttpContext.User`; no forma parte de los casos de uso ni del dominio. Api ya referencia el framework web y registra authorization.
- **Alternatives considered**: Infrastructure (obligaría a añadir una dependencia web a un proyecto de adaptadores secundarios); ApplicationCore (viola su independencia de ASP.NET Core); Host (ocultaría al adaptador dentro de la composición y reduciría su reutilización/testabilidad).

## Declaración mediante requisito nativo

- **Decision**: Usar un atributo que hereda de `AuthorizeAttribute` e implementa `IAuthorizationRequirementData`. El atributo aporta `ApiAuthorizationRequirement`; un `AuthorizationHandler` registrado por DI coordina las llamadas al port.
- **Rationale**: Integra metadata y evaluación con el middleware estándar, que produce `401`/`403`, y evita tanto filtros MVC como middleware propio.
- **Alternatives considered**: `TypeFilterAttribute` y filtro MVC (más infraestructura propia); `AuthorizeAttribute` repetido por policy (no transporta resource); middleware global con tabla de rutas (menos visible y frágil ante routing).

## Semántica de policies múltiples

- **Decision**: Normalizar y deduplicar nombres, conservar el orden declarado y exigir éxito de todas las policies únicas; detenerse en el primer fallo.
- **Rationale**: Cumple la semántica AND fijada por el spec, evita evaluaciones duplicadas y reduce trabajo sin cambiar el resultado.
- **Alternatives considered**: OR (amplía permisos contra el spec); evaluar duplicados (trabajo innecesario); evaluar todas tras un fallo (no aporta valor a una decisión booleana y aumenta latencia).

## Resource

- **Decision**: Pasar al port el nombre catalogado (`Vehicles` o `Rentals`) como resource inmutable de la operación, no una entidad cargada.
- **Rationale**: El requerimiento solicita `resource name` y las policies actuales son de capacidad, no dependientes de una instancia. Evita acceso a persistencia antes del caso de uso.
- **Alternatives considered**: Cargar `Vehicle`/`Rental` en el handler (duplica consultas y reglas); usar la ruta como resource implícito (inestable y no tipado); pasar `null` (pierde el contexto solicitado).

## Catálogo y nombres

- **Decision**: Definir dos resources y cuatro policies con nombres `Vehicles.Create`, `Vehicles.Read`, `Rentals.Create` y `Rentals.Return`; documentar una asignación uno-a-uno a las cuatro actions actuales.
- **Rationale**: Son nombres estables, explícitos y alineados con el lenguaje de flota/alquiler. Separar create/read/return permite mínimo privilegio.
- **Alternatives considered**: Una policy global `ApiAccess` (demasiado amplia); policies CRUD genéricas sin contexto (colisiones y menor auditabilidad); una policy por ruta literal (acoplamiento al transporte).

## Claims que satisfacen policies

- **Decision**: Registrar cada policy para exigir una claim de permiso cuyo valor coincide exactamente con el nombre catalogado. El tipo de claim quedará centralizado en el catálogo y documentado como `permission`.
- **Rationale**: Es determinista, permite mínimo privilegio y puede probarse sin una autoridad externa. La comparación exacta evita concesiones por prefijos o diferencias de mayúsculas.
- **Alternatives considered**: Roles amplios (menor granularidad); scopes como única representación (pueden agrupar capacidades distintas); handlers con reglas codificadas por endpoint (duplican catálogo).

## Resultados HTTP y fallos

- **Decision**: Identidad ausente/no autenticada produce `401`; identidad autenticada que no satisface una policy produce `403`; metadata inválida o nombres desconocidos fallan cerrado y nunca ejecutan la action.
- **Rationale**: Mantiene la distinción HTTP estándar y no revela qué claim concreta falta. Fail-closed es obligatorio para errores de configuración de seguridad.
- **Alternatives considered**: Devolver siempre `403` (pierde el challenge de autenticación); `500` al cliente por nombre desconocido (filtra configuración y empeora contrato); permitir continuar ante excepción (inseguro).

## Compatibilidad, Swagger y pruebas

- **Decision**: Mantener contratos de éxito/error de dominio, añadir `401`/`403` y bearer security en OpenAPI. En infrastructure tests se sustituye el esquema por autenticación determinista; no se contacta con IdentityServer.
- **Rationale**: Las pruebas siguen reproducibles y rápidas, mientras la composición prueba que `JwtAuthority` configura producción. La documentación anuncia el nuevo requisito de acceso.
- **Alternatives considered**: Usar tokens/IdentityServer reales en tests (secretos y dependencia externa); omitir OpenAPI (contrato inexacto); probar solo el handler (no demuestra cobertura de endpoints).
