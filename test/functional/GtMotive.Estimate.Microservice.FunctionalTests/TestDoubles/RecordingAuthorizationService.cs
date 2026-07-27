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

