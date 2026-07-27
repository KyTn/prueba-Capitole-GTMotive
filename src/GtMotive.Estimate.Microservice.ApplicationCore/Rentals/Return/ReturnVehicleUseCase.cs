using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.People;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;

public sealed class ReturnVehicleUseCase(
    IPersonRegistry personRegistry,
    IVehicleRepository vehicleRepository,
    IRentalRepository rentalRepository,
    IClock clock,
    IAppLogger<ReturnVehicleUseCase> logger) : IUseCase<ReturnVehicleCommand>
{
    async Task IUseCase<ReturnVehicleCommand>.Execute(ReturnVehicleCommand input)
    {
        await ExecuteAsync(input, CancellationToken.None);
    }

    public async Task<ReturnVehicleResult> ExecuteAsync(
        ReturnVehicleCommand command,
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
            return ReturnVehicleResult.Failure(
                ReturnVehicleResultType.InvalidInput,
                exception.Code,
                exception.Message);
        }

        if (!await personRegistry.ExistsAsync(personId, cancellationToken))
        {
            return ReturnVehicleResult.Failure(
                ReturnVehicleResultType.PersonNotFound,
                "person_not_found",
                "The person was not found.");
        }

        var vehicle = await vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return ReturnVehicleResult.Failure(
                ReturnVehicleResultType.VehicleNotFound,
                "vehicle_not_found",
                "The vehicle was not found.");
        }

        var rental = await rentalRepository.GetActiveByVehicleIdAsync(vehicle.Id, cancellationToken);
        if (rental is null)
        {
            logger.LogWarning("Return rejected because vehicle {VehicleId} has no active rental.", vehicle.Id);
            return ReturnVehicleResult.Failure(
                ReturnVehicleResultType.VehicleNotRented,
                "vehicle_not_rented",
                "The vehicle has no active rental.");
        }

        if (rental.PersonId != personId)
        {
            logger.LogWarning("Return rejected because rental {RentalId} has another owner.", rental.Id);
            return ReturnVehicleResult.Failure(
                ReturnVehicleResultType.RentalOwnershipConflict,
                "rental_ownership_conflict",
                "The active rental belongs to another person.");
        }

        rental.Return(clock.UtcNow);
        var closeResult = await rentalRepository.TryCloseActiveAsync(rental, cancellationToken);
        if (closeResult == CloseActiveRentalResult.Conflict)
        {
            logger.LogWarning("Return rejected because rental {RentalId} is no longer active.", rental.Id);
            return ReturnVehicleResult.Failure(
                ReturnVehicleResultType.RentalAlreadyReturned,
                "rental_already_returned",
                "The rental was already returned.");
        }

        logger.LogInformation("Rental {RentalId} returned for vehicle {VehicleId}.", rental.Id, rental.VehicleId);
        return ReturnVehicleResult.Returned(
            new RentalDto(
                rental.Id,
                rental.PersonId.Value,
                rental.VehicleId,
                rental.StartedAt,
                rental.Status.ToString().ToLowerInvariant(),
                rental.EndedAt));
    }
}
