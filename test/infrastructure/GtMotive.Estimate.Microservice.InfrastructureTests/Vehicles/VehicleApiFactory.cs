using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;

internal sealed class VehicleApiFactory : WebApplicationFactory<Program>
{
    public InMemoryVehicleRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IVehicleRepository>();
            services.RemoveAll<IClock>();
            services.AddSingleton<IVehicleRepository>(Repository);
            services.AddSingleton<IClock>(new FixedClock(new DateOnly(2026, 7, 27)));
        });
    }
}

internal sealed class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}

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

    public Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Vehicle>>(_vehicles.Values.ToArray());
    }
}
