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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Authorization;

public sealed class ConcurrentAuthorizationTests
{
    [Fact]
    public async Task ConcurrentEvaluationsKeepTheirOwnPrincipal()
    {
        var service = new RecordingAuthorizationService(
            new Dictionary<string, bool>
            {
                [AuthorizationCatalog.Policies.VehiclesRead] = true
            });
        var evaluations = Enumerable.Range(0, 32)
            .Select(index =>
            {
                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, index.ToString())],
                        "test"));
                var requirement = new ApiAuthorizationRequirement(
                    AuthorizationCatalog.Resources.Vehicles,
                    [AuthorizationCatalog.Policies.VehiclesRead]);
                var context = new AuthorizationHandlerContext(
                    [requirement],
                    principal,
                    resource: null);
                return (
                    Handler: new ApiAuthorizationHandler(service),
                    Context: context);
            })
            .ToArray();

        await Task.WhenAll(evaluations.Select(
            item => item.Handler.HandleAsync(item.Context)));

        Assert.Equal(32, service.Calls.Count);
        Assert.Equal(
            32,
            service.Calls.Select(
                call => call.User.FindFirst(ClaimTypes.NameIdentifier)?.Value).Distinct().Count());
        Assert.All(evaluations, item => Assert.True(item.Context.HasSucceeded));
    }
}

