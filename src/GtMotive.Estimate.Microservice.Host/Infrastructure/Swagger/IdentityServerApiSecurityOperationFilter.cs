/*
 * Aviso de propiedad intelectual
 *
 * Este repositorio se ha creado exclusivamente como prueba tÃ©cnica para Capitole.
 * Salvo los componentes de terceros y los derechos que pudieran haberse cedido
 * expresamente por contrato, el cÃ³digo y la documentaciÃ³n originales contenidos en
 * Ã©l son propiedad de su autor. No se autoriza su copia, reproducciÃ³n, modificaciÃ³n,
 * distribuciÃ³n, publicaciÃ³n ni explotaciÃ³n, total o parcial, sin consentimiento
 * previo y por escrito del titular de los derechos. El titular se reserva el
 * ejercicio de las acciones legales que correspondan frente a cualquier uso no
 * autorizado.
 */

using System;
using System.Linq;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GtMotive.Estimate.Microservice.Host.Infrastructure.Swagger
{
    internal sealed class IdentityServerApiSecurityOperationFilter : IOperationFilter
    {
        internal static readonly string[] OpenApiSecuritySchemesValues = ["estimate-api"];

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);

            ArgumentNullException.ThrowIfNull(context);

            var controllerAttributes = context.MethodInfo.DeclaringType is null
                ? []
                : context.MethodInfo.DeclaringType
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .ToArray();

            var methodAttributes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToArray();

            var attributes = controllerAttributes.Union(methodAttributes).ToArray();
            var hasApiAuthorization =
                context.MethodInfo.DeclaringType?.IsDefined(
                    typeof(ApiAuthorizationAttribute),
                    inherit: true) == true ||
                context.MethodInfo.IsDefined(
                    typeof(ApiAuthorizationAttribute),
                    inherit: true);

            if (attributes.Length != 0 || hasApiAuthorization)
            {
                operation.Responses.TryAdd(
                    "401",
                    new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd(
                    "403",
                    new OpenApiResponse { Description = "Forbidden" });

                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "oauth2"
                                }
                            },
                            OpenApiSecuritySchemesValues
                        }
                    }

                ];
            }
        }
    }
}
