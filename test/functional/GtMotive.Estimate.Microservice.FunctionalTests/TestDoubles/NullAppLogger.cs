using System;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class NullAppLogger<T> : IAppLogger<T>
{
    public void LogInformation(string message, params object[] args) { }

    public void LogWarning(string message, params object[] args) { }

    public void LogError(Exception exception, string message, params object[] args) { }

    public void LogDebug(string message, params object[] args) { }
}
