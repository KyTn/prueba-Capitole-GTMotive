using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.People;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class InMemoryPersonRegistry : IPersonRegistry
{
    private readonly ConcurrentDictionary<PersonId, byte> _people = new();

    public void Add(PersonId personId) => _people.TryAdd(personId, 0);

    public Task<bool> ExistsAsync(PersonId personId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_people.ContainsKey(personId));
    }
}
