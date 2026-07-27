using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.People;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;

public sealed class RentVehicleUseCase(
    IPersonRegistry personRegistry,
    IVehicleRepository vehicleRepository,
    IRentalRepository rentalRepository,
    IClock clock,
    IAppLogger<RentVehicleUseCase> logger) : IUseCase<RentVehicleCommand>
{
    async Task IUseCase<RentVehicleCommand>.Execute(RentVehicleCommand input)
    {
        await ExecuteAsync(input, CancellationToken.None);
    }

    public async Task<RentVehicleResult> ExecuteAsync(
        RentVehicleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        PersonId personId;
        try
        {
            personId = new PersonId(command.PersonId);
            if (command.VehicleId == Guid.Empty)
            {
                throw new RentalValidationException("invalid_vehicle_id", "Vehicle id is required.");
            }
        }
        catch (RentalValidationException exception)
        {
            return RentVehicleResult.Failure(
                RentVehicleResultType.InvalidInput,
                exception.Code,
                exception.Message);
        }

        if (!await personRegistry.ExistsAsync(personId, cancellationToken))
        {
            return RentVehicleResult.Failure(
                RentVehicleResultType.PersonNotFound,
                "person_not_found",
                "The person was not found.");
        }

        var vehicle = await vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return RentVehicleResult.Failure(
                RentVehicleResultType.VehicleNotFound,
                "vehicle_not_found",
                "The vehicle was not found.");
        }

        var rental = Rental.Create(Guid.NewGuid(), personId, vehicle.Id, clock.UtcNow);
        var addResult = await rentalRepository.TryAddActiveAsync(rental, cancellationToken);
        if (addResult == AddActiveRentalResult.PersonConflict)
        {
            logger.LogWarning("Rental rejected because person already has an active rental.");
            return RentVehicleResult.Failure(
                RentVehicleResultType.PersonAlreadyHasActiveRental,
                "person_already_has_active_rental",
                "The person already has an active rental.");
        }

        if (addResult == AddActiveRentalResult.VehicleConflict)
        {
            logger.LogWarning("Rental rejected because vehicle {VehicleId} is unavailable.", vehicle.Id);
            return RentVehicleResult.Failure(
                RentVehicleResultType.VehicleNotAvailable,
                "vehicle_not_available",
                "The vehicle is not available.");
        }

        logger.LogInformation("Rental {RentalId} created for vehicle {VehicleId}.", rental.Id, rental.VehicleId);
        return RentVehicleResult.Created(
            new RentalDto(
                rental.Id,
                rental.PersonId.Value,
                rental.VehicleId,
                rental.StartedAt,
                rental.Status.ToString().ToLowerInvariant()));
    }
}
