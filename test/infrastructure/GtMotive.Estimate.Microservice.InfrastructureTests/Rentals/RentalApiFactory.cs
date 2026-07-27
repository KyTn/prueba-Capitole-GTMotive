using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Common.Time;
using GtMotive.Estimate.Microservice.ApplicationCore.People;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

internal sealed class RentalApiFactory : WebApplicationFactory<Program>
{
    public RentalApiFactory()
    {
        Clock = new RentalFixedClock(
            new DateOnly(2026, 7, 27),
            new DateTimeOffset(2026, 7, 27, 10, 0, 0, TimeSpan.Zero));
    }

    public HostPersonRegistry People { get; } = new();

    public HostVehicleRepository Vehicles { get; } = new();

    public HostRentalRepository Rentals { get; } = new();

    public RentalFixedClock Clock { get; }

    public PersonId AddPerson()
    {
        var id = new PersonId(Guid.NewGuid());
        People.Add(id);
        return id;
    }

    public async Task<Vehicle> AddVehicleAsync()
    {
        var vehicle = Vehicle.Rehydrate(
            Guid.NewGuid(),
            $"{Random.Shared.Next(1000, 9999)}XYZ",
            "Seat",
            "Leon",
            new DateOnly(2024, 1, 1));
        await Vehicles.AddAsync(vehicle, CancellationToken.None);
        return vehicle;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPersonRegistry>();
            services.RemoveAll<IVehicleRepository>();
            services.RemoveAll<IRentalRepository>();
            services.RemoveAll<IClock>();
            services.AddSingleton<IPersonRegistry>(People);
            services.AddSingleton<IVehicleRepository>(Vehicles);
            services.AddSingleton<IRentalRepository>(Rentals);
            services.AddSingleton<IClock>(Clock);
        });
    }
}

internal sealed class RentalFixedClock(DateOnly today, DateTimeOffset utcNow) : IClock
{
    public DateOnly Today { get; } = today;

    public DateTimeOffset UtcNow { get; } = utcNow;
}

internal sealed class HostPersonRegistry : IPersonRegistry
{
    private readonly ConcurrentDictionary<PersonId, byte> _people = new();

    public void Add(PersonId id) => _people.TryAdd(id, 0);

    public Task<bool> ExistsAsync(PersonId personId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_people.ContainsKey(personId));
    }
}

internal sealed class HostVehicleRepository : IVehicleRepository
{
    private readonly ConcurrentDictionary<Guid, Vehicle> _vehicles = new();

    public Task<bool> ExistsByRegistrationNumberAsync(
        RegistrationNumber registrationNumber,
        CancellationToken cancellationToken) =>
        Task.FromResult(_vehicles.Values.Any(vehicle => vehicle.RegistrationNumber == registrationNumber));

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        if (!_vehicles.TryAdd(vehicle.Id, vehicle))
        {
            throw new VehicleAlreadyExistsException();
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Vehicle>>(_vehicles.Values.ToArray());

    public Task<Vehicle> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _vehicles.TryGetValue(id, out var vehicle);
        return Task.FromResult(vehicle);
    }
}

internal sealed class HostRentalRepository : IRentalRepository
{
    private readonly object _sync = new();
    private readonly List<Rental> _rentals = new();

    public IReadOnlyList<Rental> Rentals
    {
        get
        {
            lock (_sync)
            {
                return _rentals.ToArray();
            }
        }
    }

    public Task<AddActiveRentalResult> TryAddActiveAsync(
        Rental rental,
        CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_rentals.Any(item => item.Status == RentalStatus.Active && item.PersonId == rental.PersonId))
            {
                return Task.FromResult(AddActiveRentalResult.PersonConflict);
            }

            if (_rentals.Any(item => item.Status == RentalStatus.Active && item.VehicleId == rental.VehicleId))
            {
                return Task.FromResult(AddActiveRentalResult.VehicleConflict);
            }

            _rentals.Add(rental);
            return Task.FromResult(AddActiveRentalResult.Created);
        }
    }
}
