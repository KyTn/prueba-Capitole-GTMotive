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
using System.Reflection;
using GtMotive.Estimate.Microservice.Api;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class EndpointAuthorizationCoverageTests
{
    [Fact]
    public void EveryBusinessActionHasOneCatalogedDeclarationAndIsNotAnonymous()
    {
        var actions = typeof(ApiConfiguration).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToArray();

        Assert.Equal(4, actions.Length);
        foreach (var action in actions)
        {
            Assert.False(action.IsDefined(typeof(AllowAnonymousAttribute), inherit: true));
            var declaration = Assert.Single(
                action.GetCustomAttributes<ApiAuthorizationAttribute>(inherit: true));
            Assert.True(AuthorizationCatalog.IsKnownResource(declaration.ResourceName));
            Assert.All(
                declaration.PolicyNames,
                policy => Assert.True(AuthorizationCatalog.IsKnownPolicy(policy)));
        }
    }
}
