using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Vehicles.Events;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;

public sealed class CreateVehicleHandler(
    CreateVehicleUseCase useCase,
    IBusFactory busFactory,
    ITelemetry telemetry) : IRequestHandler<CreateVehicleCommand, CreateVehicleResult>
{
    public async Task<CreateVehicleResult> Handle(
        CreateVehicleCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var stopwatch = Stopwatch.StartNew();
        var outcome = "error";
        try
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);
            outcome = result.Type == CreateVehicleResultType.Created ? "success" : "rejected";
            if (result.Type == CreateVehicleResultType.Created)
            {
                var domainEvent = new VehicleCreated(result.Vehicle.Id);
                await busFactory.GetClient(domainEvent.GetType()).Send(domainEvent);
            }

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
            UseCaseTelemetry.Track(telemetry, "CreateVehicle", outcome, stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
