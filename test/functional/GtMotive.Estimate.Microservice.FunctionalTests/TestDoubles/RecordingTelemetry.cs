using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class RecordingTelemetry : ITelemetry
{
    private readonly ConcurrentQueue<(string Name, IReadOnlyDictionary<string, string> Properties)> _events = new();
    private readonly ConcurrentQueue<(string Name, double Value, IReadOnlyDictionary<string, string> Properties)> _metrics = new();

    public IReadOnlyCollection<(string Name, IReadOnlyDictionary<string, string> Properties)> Events =>
        _events.ToArray();

    public IReadOnlyCollection<(string Name, double Value, IReadOnlyDictionary<string, string> Properties)> Metrics =>
        _metrics.ToArray();

    public void TrackEvent(
        string eventName,
        IDictionary<string, string> properties = null,
        IDictionary<string, double> metrics = null) =>
        _events.Enqueue((eventName, new Dictionary<string, string>(properties ?? new Dictionary<string, string>())));

    public void TrackMetric(
        string name,
        double value,
        IDictionary<string, string> properties = null) =>
        _metrics.Enqueue((name, value, new Dictionary<string, string>(properties ?? new Dictionary<string, string>())));
}
