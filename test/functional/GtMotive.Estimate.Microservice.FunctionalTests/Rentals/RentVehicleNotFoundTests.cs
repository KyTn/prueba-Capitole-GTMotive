using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class RentVehicleNotFoundTests
{
    [Fact]
    public async Task Execute_UnknownPersonDoesNotCreateRental()
    {
        var scenario = new RentalScenario();
        var vehicle = await scenario.AddVehicleAsync();

        var result = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(Guid.NewGuid(), vehicle.Id),
            CancellationToken.None);

        Assert.Equal(RentVehicleResultType.PersonNotFound, result.Type);
        Assert.Empty(scenario.Rentals.Rentals);
    }

    [Fact]
    public async Task Execute_UnknownVehicleDoesNotCreateRental()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();

        var result = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(person.Value, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(RentVehicleResultType.VehicleNotFound, result.Type);
        Assert.Empty(scenario.Rentals.Rentals);
    }
}
