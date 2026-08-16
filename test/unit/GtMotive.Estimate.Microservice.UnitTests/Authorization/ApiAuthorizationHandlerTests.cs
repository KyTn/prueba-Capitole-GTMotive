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
using Microsoft.AspNetCore.Authorization;
using Xunit;
using DomainAuthorizationService =
    GtMotive.Estimate.Microservice.Domain.Interfaces.IAuthorizationService;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class ApiAuthorizationHandlerTests
{
    [Fact]
    public async Task PoliciesAreAndCombinedAndShortCircuited()
    {
        var service = new RecordingAuthorizationService(true, false, true);
        var requirement = Requirement(
            AuthorizationCatalog.Policies.VehiclesRead,
            AuthorizationCatalog.Policies.VehiclesCreate,
            AuthorizationCatalog.Policies.VehiclesRead);
        var context = CreateContext(requirement);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasFailed);
        Assert.Equal(
            [AuthorizationCatalog.Policies.VehiclesRead,
             AuthorizationCatalog.Policies.VehiclesCreate],
            service.Calls);
    }

    [Fact]
    public async Task AllSuccessfulPoliciesSatisfyRequirement()
    {
        var service = new RecordingAuthorizationService(true, true);
        var requirement = Requirement(
            AuthorizationCatalog.Policies.VehiclesRead,
            AuthorizationCatalog.Policies.VehiclesCreate);
        var context = CreateContext(requirement);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task PolicyForDifferentResourceFailsClosed()
    {
        var service = new RecordingAuthorizationService(true);
        var requirement = Requirement(AuthorizationCatalog.Policies.RentalsCreate);
        var context = CreateContext(requirement);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasFailed);
        Assert.Empty(service.Calls);
    }

    private static ApiAuthorizationRequirement Requirement(params string[] policies) =>
        new(AuthorizationCatalog.Resources.Vehicles, policies);

    private static AuthorizationHandlerContext CreateContext(
        ApiAuthorizationRequirement requirement) =>
        new(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity([], "test")),
            resource: null);

    private sealed class RecordingAuthorizationService(params bool[] outcomes)
        : DomainAuthorizationService
    {
        private int _index;

        public List<string> Calls { get; } = [];

        public Task<bool> Authorize(
            ClaimsPrincipal user,
            object resource,
            string policyName)
        {
            Assert.Equal(AuthorizationCatalog.Resources.Vehicles, resource);
            Calls.Add(policyName);
            return Task.FromResult(outcomes[_index++]);
        }
    }
}
