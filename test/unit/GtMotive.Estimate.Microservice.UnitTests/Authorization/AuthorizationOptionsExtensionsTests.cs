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

using System.Linq;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class AuthorizationOptionsExtensionsTests
{
    [Fact]
    public void ConfigureRegistersEveryCatalogPolicyWithMatchingPermission()
    {
        var options = new AuthorizationOptions();

        AuthorizationOptionsExtensions.Configure(options);

        foreach (var policyName in AuthorizationCatalog.Policies.All)
        {
            var policy = options.GetPolicy(policyName);
            var requirement = Assert.Single(policy.Requirements.OfType<ClaimsAuthorizationRequirement>());
            Assert.Equal(AuthorizationCatalog.PermissionClaimType, requirement.ClaimType);
            Assert.Equal([policyName], requirement.AllowedValues);
        }
    }
}

