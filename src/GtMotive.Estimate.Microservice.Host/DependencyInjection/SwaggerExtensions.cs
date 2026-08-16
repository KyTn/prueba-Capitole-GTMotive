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
using System.Collections.Generic;
using System.Reflection;
using GtMotive.Estimate.Microservice.Host.Configuration;
using GtMotive.Estimate.Microservice.Host.Infrastructure.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace GtMotive.Estimate.Microservice.Host.DependencyInjection
{
    internal static class SwaggerExtensions
    {
        private static string AssemblyName => Assembly.GetEntryAssembly().GetName().Name;

        private static string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public static IServiceCollection AddSwagger(
            this IServiceCollection services,
            AppSettings settings,
            IConfiguration configuration)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(
                options =>
                {
                    options.CustomSchemaIds(type => type.ToString());
                    options.SwaggerDoc($"v{AssemblyVersion}", new OpenApiInfo
                    {
                        Title = $"{AssemblyName} API",
                        Version = $"v{AssemblyVersion}",
                    });

                    if (configuration.GetValue<string>("Swagger:EnableTryIt") == "Yes")
                    {
                        // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.OAuth2,
                            Name = "oauth2",
                            Flows = configuration.GetValue<string>("Swagger:AuthFlow") == "AuthorizationCode"
                                ? new OpenApiOAuthFlows
                                {
                                    AuthorizationCode = new OpenApiOAuthFlow
                                    {
                                        AuthorizationUrl = new Uri($"{settings.JwtAuthority}/connect/authorize"),
                                        Scopes = new Dictionary<string, string>
                                        {
                                            ["estimate-public-scope"] = "estimate-api"
                                        },
                                        TokenUrl = new Uri($"{settings.JwtAuthority}/connect/token")
                                    }
                                }
                                : new OpenApiOAuthFlows()
                                {
                                    ClientCredentials = new OpenApiOAuthFlow()
                                    {
                                        Scopes = new Dictionary<string, string>
                                        {
                                            ["estimate-public-scope"] = "estimate-api"
                                        },
                                        TokenUrl = new Uri($"{settings.JwtAuthority}/connect/token")
                                    }
                                }
                        });

                        options.OperationFilter<IdentityServerApiSecurityOperationFilter>();
                    }
                });

            return services;
        }

        public static IApplicationBuilder UseSwaggerInApplication(
            this IApplicationBuilder app,
            PathBase pathBase,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(pathBase);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(options =>
            {
                if (!pathBase.IsDefault)
                {
                    options.SerializeAsV2 = true;
                    options.RouteTemplate = "swagger/{documentName}/swagger.json";
                    options.PreSerializeFilters.Add((document, request) =>
                    {
                        document.Servers =
                        [
                            new OpenApiServer
                            {
                                Url = $"{request.Scheme}://{request.Host.Value}{pathBase.CurrentWithoutTrailingSlash}"
                            }

                        ];
                    });
                }
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            var url = pathBase.IsDefault
                ? $"/swagger/v{AssemblyVersion}/swagger.json"
                : $"{pathBase.CurrentWithoutTrailingSlash}/swagger/v{AssemblyVersion}/swagger.json";

            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint(url, $"{AssemblyName} API V{AssemblyVersion}");

                    if (configuration.GetValue<string>("Swagger:EnableTryIt") == "No")
                    {
                        options.SupportedSubmitMethods();
                    }

                    options.OAuthClientId("client-gtestimate-swagger");
                    options.OAuthClientSecret("gtmotive");
                    options.OAuthScopeSeparator(" ");
                });

            return app;
        }
    }
}
