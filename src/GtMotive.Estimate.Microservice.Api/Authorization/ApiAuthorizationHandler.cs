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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

public sealed class ApiAuthorizationHandler(
    Domain.Interfaces.IAuthorizationService authorizationService)
    : AuthorizationHandler<ApiAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiAuthorizationRequirement requirement)
    {
        if (!AuthorizationCatalog.IsKnownResource(requirement.ResourceName) ||
            requirement.PolicyNames.Count == 0 ||
            requirement.PolicyNames.Any(policyName =>
                !AuthorizationCatalog.IsPolicyForResource(
                    policyName,
                    requirement.ResourceName)))
        {
            context.Fail();
            return;
        }

        foreach (var policyName in requirement.PolicyNames)
        {
            if (!await authorizationService.Authorize(
                    context.User,
                    requirement.ResourceName,
                    policyName))
            {
                context.Fail();
                return;
            }
        }

        context.Succeed(requirement);
    }
}

