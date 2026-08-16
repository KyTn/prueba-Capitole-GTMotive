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
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using DomainAuthorizationService =
    GtMotive.Estimate.Microservice.Domain.Interfaces.IAuthorizationService;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class AuthorizationPerformanceTests
{
    [Fact]
    public async Task InProcessDecisionP95IsUnderOneHundredMilliseconds()
    {
        var handler = new ApiAuthorizationHandler(new AllowAuthorizationService());
        var requirement = new ApiAuthorizationRequirement(
            AuthorizationCatalog.Resources.Vehicles,
            [AuthorizationCatalog.Policies.VehiclesRead]);
        var durations = new List<double>();

        for (var index = 0; index < 100; index++)
        {
            var context = new AuthorizationHandlerContext(
                [requirement],
                new ClaimsPrincipal(new ClaimsIdentity([], "performance-test")),
                resource: null);
            var stopwatch = Stopwatch.StartNew();
            await handler.HandleAsync(context);
            stopwatch.Stop();
            durations.Add(stopwatch.Elapsed.TotalMilliseconds);
            Assert.True(context.HasSucceeded);
        }

        var ordered = durations.OrderBy(value => value).ToArray();
        Assert.True(ordered[94] < 100, $"Authorization p95 was {ordered[94]:F2} ms.");
    }

    private sealed class AllowAuthorizationService : DomainAuthorizationService
    {
        public Task<bool> Authorize(
            ClaimsPrincipal user,
            object resource,
            string policyName) => Task.FromResult(true);
    }
}
