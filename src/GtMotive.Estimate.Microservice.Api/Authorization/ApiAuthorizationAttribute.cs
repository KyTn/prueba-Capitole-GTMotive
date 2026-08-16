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
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true)]
public sealed class ApiAuthorizationAttribute :
    AuthorizeAttribute,
    IAuthorizationRequirementData
{
    public ApiAuthorizationAttribute(string resourceName, params string[] policyNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentNullException.ThrowIfNull(policyNames);

        var normalizedPolicies = policyNames
            .Select(policyName =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
                return policyName.Trim();
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedPolicies.Length == 0)
        {
            throw new ArgumentException(
                "At least one authorization policy is required.",
                nameof(policyNames));
        }

        ResourceName = resourceName.Trim();
        PolicyNames = Array.AsReadOnly(normalizedPolicies);
    }

    public string ResourceName { get; }

    public IReadOnlyList<string> PolicyNames { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new ApiAuthorizationRequirement(ResourceName, PolicyNames);
    }
}

