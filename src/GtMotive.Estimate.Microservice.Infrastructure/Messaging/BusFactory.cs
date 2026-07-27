using System;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Infrastructure.Messaging;

public sealed class BusFactory(IBus bus) : IBusFactory
{
    public IBus GetClient(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        return bus;
    }
}
