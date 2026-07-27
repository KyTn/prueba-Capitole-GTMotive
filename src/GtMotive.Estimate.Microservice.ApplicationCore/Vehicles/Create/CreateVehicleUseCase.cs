using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;

public sealed class CreateVehicleUseCase(
    IVehicleRepository repository,
    IClock clock,
    IAppLogger<CreateVehicleUseCase> logger) : IUseCase<CreateVehicleCommand>
{
    async Task IUseCase<CreateVehicleCommand>.Execute(CreateVehicleCommand input)
    {
        await ExecuteAsync(input, CancellationToken.None);
    }

    public async Task<CreateVehicleResult> ExecuteAsync(
        CreateVehicleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Vehicle vehicle;
        try
        {
            vehicle = Vehicle.Create(
                Guid.NewGuid(),
                command.RegistrationNumber,
                command.Brand,
                command.Model,
                command.ManufactureDate,
                clock.Today);
        }
        catch (VehicleValidationException exception)
        {
            logger.LogWarning("Vehicle creation rejected with code {Code}.", exception.Code);
            var type = exception.Code == VehicleErrorCodes.VehicleTooOld
                ? CreateVehicleResultType.VehicleTooOld
                : CreateVehicleResultType.InvalidInput;
            return CreateVehicleResult.Failure(type, exception.Code, exception.Message);
        }

        if (await repository.ExistsByRegistrationNumberAsync(vehicle.RegistrationNumber, cancellationToken))
        {
            return CreateVehicleResult.Failure(
                CreateVehicleResultType.VehicleAlreadyExists,
                "vehicle_already_exists",
                "A vehicle with this registration number already exists.");
        }

        try
        {
            await repository.AddAsync(vehicle, cancellationToken);
        }
        catch (VehicleAlreadyExistsException exception)
        {
            return CreateVehicleResult.Failure(
                CreateVehicleResultType.VehicleAlreadyExists,
                "vehicle_already_exists",
                exception.Message);
        }

        logger.LogInformation("Vehicle {VehicleId} created.", vehicle.Id);
        return CreateVehicleResult.Created(
            new VehicleDto(
                vehicle.Id,
                vehicle.RegistrationNumber.Value,
                vehicle.Brand,
                vehicle.Model,
                vehicle.ManufactureDate));
    }
}
