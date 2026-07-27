using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;

public interface IClock
{
    DateOnly Today { get; }
}
