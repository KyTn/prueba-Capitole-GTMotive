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

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Authorization;

public sealed class MultiPolicyAuthorizationTests
{
    [Fact]
    public async Task HandlerPassesSamePrincipalAndResourceToAllPolicies()
    {
        var service = new RecordingAuthorizationService(
            new Dictionary<string, bool>
            {
                [AuthorizationCatalog.Policies.VehiclesRead] = true,
                [AuthorizationCatalog.Policies.VehiclesCreate] = true
            });
        var requirement = new ApiAuthorizationRequirement(
            AuthorizationCatalog.Resources.Vehicles,
            [
                AuthorizationCatalog.Policies.VehiclesRead,
                AuthorizationCatalog.Policies.VehiclesCreate
            ]);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([], "test"));
        var context = new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.Equal(2, service.Calls.Count);
        Assert.All(service.Calls, call => Assert.Same(principal, call.User));
        Assert.All(
            service.Calls,
            call => Assert.Equal(AuthorizationCatalog.Resources.Vehicles, call.Resource));
    }

    [Fact]
    public async Task HandlerFailsClosedOnFirstDeniedPolicy()
    {
        var service = new RecordingAuthorizationService(
            new Dictionary<string, bool>
            {
                [AuthorizationCatalog.Policies.VehiclesRead] = false,
                [AuthorizationCatalog.Policies.VehiclesCreate] = true
            });
        var requirement = new ApiAuthorizationRequirement(
            AuthorizationCatalog.Resources.Vehicles,
            [
                AuthorizationCatalog.Policies.VehiclesRead,
                AuthorizationCatalog.Policies.VehiclesCreate
            ]);
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity([], "test")),
            resource: null);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasFailed);
        Assert.Single(service.Calls);
    }
}

