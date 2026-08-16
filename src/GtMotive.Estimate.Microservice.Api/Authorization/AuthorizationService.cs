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
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

public sealed class AuthorizationService : IAuthorizationService
{
    public Task<bool> Authorize(
        ClaimsPrincipal user,
        object resource,
        string policyName)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var resourceName = resource as string;
        var authorized =
            user.Identity?.IsAuthenticated == true &&
            AuthorizationCatalog.IsPolicyForResource(policyName, resourceName) &&
            user.Claims.Any(claim =>
                string.Equals(
                    claim.Type,
                    AuthorizationCatalog.PermissionClaimType,
                    StringComparison.Ordinal) &&
                string.Equals(claim.Value, policyName, StringComparison.Ordinal));

        return Task.FromResult(authorized);
    }
}
