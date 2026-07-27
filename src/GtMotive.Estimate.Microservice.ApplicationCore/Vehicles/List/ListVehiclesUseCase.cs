using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed class ListVehiclesUseCase(
    IVehicleRepository repository,
    IAppLogger<ListVehiclesUseCase> logger)
{
    public async Task<ListVehiclesResult> ExecuteAsync(CancellationToken cancellationToken)
    {
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
