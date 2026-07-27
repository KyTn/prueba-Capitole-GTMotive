using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

internal sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly ConcurrentDictionary<string, Vehicle> _vehicles = new();

    public int Count => _vehicles.Count;

    public Task<bool> ExistsByRegistrationNumberAsync(
        RegistrationNumber registrationNumber,
        CancellationToken cancellationToken) =>
        Task.FromResult(_vehicles.ContainsKey(registrationNumber.Value));

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        if (!_vehicles.TryAdd(vehicle.RegistrationNumber.Value, vehicle))
        {
            throw new VehicleAlreadyExistsException();
        }

        return Task.CompletedTask;
    }
}
