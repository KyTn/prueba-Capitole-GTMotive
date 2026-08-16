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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class RecordingAuthorizationService(
    IReadOnlyDictionary<string, bool> outcomes) : IAuthorizationService
{
    private readonly ConcurrentQueue<AuthorizationCall> _calls = new();

    public IReadOnlyCollection<AuthorizationCall> Calls => _calls.ToArray();

    public Task<bool> Authorize(
        ClaimsPrincipal user,
        object resource,
        string policyName)
    {
        _calls.Enqueue(new AuthorizationCall(user, resource, policyName));
        return Task.FromResult(
            outcomes.TryGetValue(policyName, out var outcome) && outcome);
    }

    internal sealed record AuthorizationCall(
        ClaimsPrincipal User,
        object Resource,
        string PolicyName);
}

