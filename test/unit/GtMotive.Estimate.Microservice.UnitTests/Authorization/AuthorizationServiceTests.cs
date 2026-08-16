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

using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class AuthorizationServiceTests
{
    [Fact]
    public async Task AuthorizeRequiresAuthenticatedPrincipalMatchingResourceAndPermission()
    {
        var service = new AuthorizationService();
        var user = Principal(
            new Claim(
                AuthorizationCatalog.PermissionClaimType,
                AuthorizationCatalog.Policies.VehiclesRead));

        Assert.True(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Vehicles,
            AuthorizationCatalog.Policies.VehiclesRead));
        Assert.False(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Rentals,
            AuthorizationCatalog.Policies.VehiclesRead));
        Assert.False(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Vehicles,
            AuthorizationCatalog.Policies.VehiclesCreate));
    }

    [Fact]
    public async Task AuthorizeRejectsUnauthenticatedPrincipal()
    {
        var service = new AuthorizationService();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(
                AuthorizationCatalog.PermissionClaimType,
                AuthorizationCatalog.Policies.VehiclesRead)]));

        Assert.False(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Vehicles,
            AuthorizationCatalog.Policies.VehiclesRead));
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));
}

