using System.Collections.Generic;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

internal static class UseCaseTelemetry
{
    internal const string CompletedEvent = "UseCaseCompleted";
    internal const string DurationMetric = "UseCaseDurationMs";

    internal static void Track(ITelemetry telemetry, string operation, string outcome, double durationMs)
    {
        try
        {
            var properties = new Dictionary<string, string>
            {
                ["operation"] = operation,
                ["outcome"] = outcome,
            };
            telemetry.TrackEvent(CompletedEvent, properties);
            telemetry.TrackMetric(
                DurationMetric,
                durationMs,
                new Dictionary<string, string> { ["operation"] = operation });
        }
        catch
        {
            // Telemetry is observational and must not change the use-case result.
        }
    }
}
