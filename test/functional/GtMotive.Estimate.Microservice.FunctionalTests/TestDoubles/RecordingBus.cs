using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class RecordingBus : IBus
{
    private readonly ConcurrentQueue<object> _messages = new();

    public IReadOnlyCollection<object> Messages => _messages.ToArray();

    public Task Send(object message)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}
