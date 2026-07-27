using System;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}
