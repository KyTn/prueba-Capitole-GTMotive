using System;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;

namespace GtMotive.Estimate.Microservice.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
