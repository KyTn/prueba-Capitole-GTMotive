using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Infrastructure.Messaging;

public sealed class Bus(IAppLogger<Bus> logger) : IBus
{
    public Task Send(object message)
    {
        ArgumentNullException.ThrowIfNull(message);
        logger.LogInformation("Domain event {EventType} published.", message.GetType().Name);
        return Task.CompletedTask;
    }
}
