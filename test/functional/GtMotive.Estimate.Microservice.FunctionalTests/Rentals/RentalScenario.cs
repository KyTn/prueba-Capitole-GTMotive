using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

internal sealed class RentalScenario
{
    public RentalScenario()
    {
        UseCase = new RentVehicleUseCase(
            People,
            Vehicles,
            Rentals,
            new FixedClock(new DateOnly(2026, 7, 27)),
            new NullAppLogger<RentVehicleUseCase>());
    }

    public InMemoryPersonRegistry People { get; } = new();

    public InMemoryVehicleRepository Vehicles { get; } = new();

    public InMemoryRentalRepository Rentals { get; } = new();

    public RentVehicleUseCase UseCase { get; }

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
            $"{Random.Shared.Next(1000, 9999)}ABC",
            "Toyota",
            "Corolla",
            new DateOnly(2024, 1, 1));
        await Vehicles.AddAsync(vehicle, CancellationToken.None);
        return vehicle;
    }
}
