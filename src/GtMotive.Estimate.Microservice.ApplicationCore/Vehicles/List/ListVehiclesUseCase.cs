using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed class ListVehiclesUseCase(
    IVehicleRepository repository,
    IAppLogger<ListVehiclesUseCase> logger) : IUseCase<ListVehiclesQuery>
{
    async Task IUseCase<ListVehiclesQuery>.Execute(ListVehiclesQuery input)
    {
        await ExecuteAsync(input, CancellationToken.None);
    }

    public Task<ListVehiclesResult> ExecuteAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(new ListVehiclesQuery(), cancellationToken);

    public async Task<ListVehiclesResult> ExecuteAsync(
        ListVehiclesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var vehicles = await repository.GetAllAsync(cancellationToken);
        var result = vehicles
            .Select(vehicle => new VehicleDto(
                vehicle.Id,
                vehicle.RegistrationNumber.Value,
                vehicle.Brand,
                vehicle.Model,
                vehicle.ManufactureDate))
            .ToArray();

        logger.LogInformation("{VehicleCount} vehicles listed.", result.Length);
        return new ListVehiclesResult(result);
    }
}
