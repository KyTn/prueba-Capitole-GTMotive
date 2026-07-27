using System;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class RecordingBusFactory : IBusFactory
{
    public RecordingBus Bus { get; } = new();

    public Type LastEventType { get; private set; }

    public IBus GetClient(Type eventType)
    {
        LastEventType = eventType;
        return Bus;
    }
}
