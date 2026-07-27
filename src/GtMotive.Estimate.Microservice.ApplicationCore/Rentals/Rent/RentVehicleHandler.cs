using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals.Events;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;

public sealed class RentVehicleHandler(
    RentVehicleUseCase useCase,
    IBusFactory busFactory,
    ITelemetry telemetry) : IRequestHandler<RentVehicleCommand, RentVehicleResult>
{
    public async Task<RentVehicleResult> Handle(
        RentVehicleCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var stopwatch = Stopwatch.StartNew();
        var outcome = "error";
        try
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);
            outcome = result.Type == RentVehicleResultType.Created ? "success" : "rejected";
            if (result.Type == RentVehicleResultType.Created)
            {
                var rental = result.Rental;
                var domainEvent = new VehicleRented(
                    rental.Id,
                    rental.VehicleId,
                    rental.PersonId,
                    rental.StartedAt);
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
            UseCaseTelemetry.Track(telemetry, "RentVehicle", outcome, stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
