using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed class ListVehiclesHandler(
    ListVehiclesUseCase useCase,
    ITelemetry telemetry) : IRequestHandler<ListVehiclesQuery, ListVehiclesResult>
{
    public async Task<ListVehiclesResult> Handle(
        ListVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var stopwatch = Stopwatch.StartNew();
        var outcome = "error";
        try
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);
            outcome = "success";
            return result;
        }
        catch (OperationCanceledException)
        {
            outcome = "cancelled";
            throw;
        }
        finally
        {
            stopwatch.Stop();
            UseCaseTelemetry.Track(telemetry, "ListVehicles", outcome, stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
