# Implementation Plan: Autorización de endpoints mediante policies y resources

**Branch**: `006-api-authorization-policies` | **Date**: 2026-07-27 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/006-api-authorization-policies/spec.md`

## Summary

Proteger los cuatro endpoints MVC existentes con una declaración reutilizable que contiene un resource y una o varias policies. El pipeline autenticará el JWT mediante `AppSettings:JwtAuthority`; el atributo aportará un requisito nativo mediante `IAuthorizationRequirementData` y un `AuthorizationHandler` evaluará cada policy —con semántica AND y deduplicación— mediante el contrato de dominio `IAuthorizationService`. Host conservará autenticación y composición, y el middleware estándar producirá `401`/`403` antes de MediatR y los casos de uso.

## Technical Context

**Language/Version**: C# / .NET 9 (`global.json`: SDK 9.0.203)  
**Primary Dependencies**: ASP.NET Core Authorization, IdentityServer4.AccessTokenValidation 3.0.1, MediatR 10.0.1 y Microsoft.Extensions.DependencyInjection; sin paquetes nuevos  
**Storage**: N/A; no cambian MongoDB, documentos, índices ni repositorios  
**Testing**: xUnit 2.9.2, Microsoft.AspNetCore.Mvc.Testing 9.0.0, reflexión y dobles manuales de autorización/autenticación  
**Target Platform**: Servicio HTTP Linux, ejecutable localmente con .NET 9 y en contenedores oficiales .NET 9  
**Project Type**: Microservicio web con Domain, ApplicationCore, Infrastructure, Api y Host  
**Performance Goals**: El 95 % de las decisiones añade como máximo 100 ms y cada policy única se evalúa como máximo una vez por solicitud  
**Constraints**: `JwtAuthority` es obligatorio; validación fail-closed; policies múltiples con AND; un resource no vacío; cero tokens/claims sensibles en respuestas o logs; contratos autorizados compatibles; `/health/live` fuera de alcance  
**Scale/Scope**: Cuatro endpoints, cuatro policies, dos resources, un atributo, un requisito, un handler, un servicio de autorización, configuración DI/OpenAPI y tres categorías de pruebas

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1 design.*

- **Dependency direction — PASS**: Domain conserva el port `IAuthorizationService` sin implementación concreta. Api posee metadata, requisito, handler y adaptación de autorización; Host mantiene `JwtAuthority`, autenticación y composición. ApplicationCore y los casos de uso no reciben dependencias HTTP.
- **Domain invariants — PASS**: La feature no modifica estado ni reglas de vehículos/alquileres. La autorización corta el pipeline antes de MediatR; ninguna denegación puede sortear o duplicar invariantes.
- **Use cases and contracts — PASS**: No se crean acciones de negocio ni mensajes nuevos. Los cuatro controllers conservan su mapeo a los `IRequest<TResult>` existentes y la cancelación; solo cambia la puerta de acceso HTTP y se añaden `401`/`403`.
- **Test matrix — PASS**: Unit valida catálogo, atributo, requisito, handler y semántica AND; Functional valida handler+port sin Host y demuestra que no se envía a MediatR; Infrastructure cruza Host/JWT/HTTP para los cuatro endpoints.
- **Reproducibility — PASS**: No se añaden paquetes ni servicios persistentes. Las pruebas usan un esquema de autenticación controlado y configuración de autoridad de prueba, sin secretos reales.
- **Events and observability — PASS**: La autorización ocurre antes de handlers; por ello los rechazos no publican eventos ni registran telemetría de caso de uso. Logs de seguridad, si se emiten, se limitan a nombres catalogados y resultado.
- **Quality and simplicity — PASS**: Se reutilizan los middleware de autenticación/autorización, `IAuthorizationRequirementData`, `AuthorizationHandler<T>` y el port existente. No hay middleware ni filtro propio; catálogo y pruebas de reflexión impiden divergencias.

### Post-design re-check

El catálogo define nombres cerrados y su asignación completa; el requisito nativo falla cerrado y el middleware estándar separa `401` de `403`. La autenticación sigue en Host con `JwtAuthority`; el handler consume el principal validado y coordina el port. No se introduce acceso a almacenamiento ni reglas de dominio, y los contratos HTTP autorizados permanecen compatibles.

## Project Structure

### Documentation (this feature)

```text
specs/006-api-authorization-policies/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── authorization-catalog.md
│   └── http-authorization.md
└── tasks.md                    # Creado posteriormente por /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── GtMotive.Estimate.Microservice.Domain/
│   └── Interfaces/IAuthorizationService.cs          # port existente, sin cambios de firma
├── GtMotive.Estimate.Microservice.Api/
│   ├── Authorization/
│   │   ├── ApiAuthorizationAttribute.cs             # resource + policyNames
│   │   ├── ApiAuthorizationRequirement.cs           # resource + policies
│   │   ├── ApiAuthorizationHandler.cs               # AND y fail-closed
│   │   ├── AuthorizationService.cs                  # adaptador del port al motor estándar
│   │   ├── AuthorizationCatalog.cs                  # nombres tipados y asignaciones
│   │   └── AuthorizationOptionsExtensions.cs        # registro de policies
│   ├── Vehicles/{Create,List}/*Controller.cs
│   ├── Rentals/{Rent,Return}/*Controller.cs
│   └── ApiConfiguration.cs                          # DI del handler y port
└── GtMotive.Estimate.Microservice.Host/
    ├── Program.cs                                   # JwtAuthority y pipeline
    └── Infrastructure/Swagger/
        └── IdentityServerApiSecurityOperationFilter.cs

test/
├── unit/GtMotive.Estimate.Microservice.UnitTests/Authorization/
├── functional/GtMotive.Estimate.Microservice.FunctionalTests/Authorization/
└── infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests/Authorization/
```

**Structure Decision**: Se mantiene la solución hexagonal existente. Domain conserva únicamente el contrato suministrado; Api implementa el adaptador de autorización por ser una preocupación del transporte HTTP y contiene la metadata de endpoints; Host sigue siendo dueño de autenticación, configuración y pipeline. No se modifica ApplicationCore ni Infrastructure porque la autorización precede a los casos de uso y no necesita un adaptador externo adicional.

## Design Sequence

1. Materializar `AuthorizationCatalog` con resources `Vehicles` y `Rentals`, policies `Vehicles.Create`, `Vehicles.Read`, `Rentals.Create` y `Rentals.Return`, y la asignación exacta descrita en [contracts/authorization-catalog.md](contracts/authorization-catalog.md).
2. Completar `AuthorizationOptionsExtensions` para registrar cada nombre de policy como requisito comprobable. El catálogo será la única fuente de nombres para configuración, atributos y pruebas.
3. Implementar el adaptador `AuthorizationService` del port de Domain delegando en el motor estándar de autorización; devolver `false` ante resultado no satisfactorio y no capturar fallos de programación/configuración como concesiones.
4. Crear `ApiAuthorizationAttribute` aplicable a métodos/clases, con un resource obligatorio y uno o varios `policyNames`; normalizar espacios, rechazar vacíos y exponer metadata inmutable.
5. Hacer que el atributo implemente `IAuthorizationRequirementData`, crear `ApiAuthorizationRequirement` y registrar un `AuthorizationHandler` que llame secuencialmente a `IAuthorizationService.Authorize(user, resource, policy)`, deteniéndose en el primer `false`; el middleware estándar resolverá `401`/`403`.
6. Aplicar las cuatro asignaciones a las actions existentes y retirar `[AllowAnonymous]`, sin cambiar rutas, requests, comandos/query, presenters ni cancelación.
7. Mantener `AppSettings:JwtAuthority` como autoridad única del esquema JWT en Host, validar configuración no nula/no vacía durante arranque y conservar `UseAuthentication()` antes de `UseAuthorization()`/controllers.
8. Actualizar Swagger/OpenAPI para declarar bearer security y respuestas `401`/`403` en las cuatro operaciones; asegurar que el filtro de seguridad reconoce el nuevo atributo además de la metadata estándar.
9. Añadir unit tests de constructores/metadata, catálogo completo, nombres conocidos, deduplicación, orden/cortocircuito y adaptación del servicio.
10. Añadir functional tests con identidad y servicio dobles que demuestren AND, resource compartido, fail-closed y que una denegación no ejecuta MediatR/caso de uso ni eventos/telemetría.
11. Añadir infrastructure tests parametrizados para los cuatro endpoints: sin credencial (`401`), autenticado sin policy (`403`) y autorizado (contrato previo); sustituir autenticación real por un esquema determinista dentro de la factoría de pruebas.
12. Ejecutar restore, build/analyzers, unit, functional e infrastructure; validar cobertura por reflexión, OpenAPI, autoridad requerida y los recorridos local/Docker de [quickstart.md](quickstart.md).

## Complexity Tracking

No hay violaciones constitucionales que requieran justificación.
