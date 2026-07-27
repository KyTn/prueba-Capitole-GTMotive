using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class ReturnVehicleOwnershipTests
{
    [Fact]
    public async Task Execute_WrongOwnerKeepsRentalActive()
    {
        var scenario = new RentalScenario();
        var owner = scenario.AddPerson();
        var other = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();
        await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(owner.Value, vehicle.Id),
            CancellationToken.None);

        var result = await scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(other.Value, vehicle.Id),
            CancellationToken.None);

        Assert.Equal(ReturnVehicleResultType.RentalOwnershipConflict, result.Type);
        Assert.Equal(RentalStatus.Active, Assert.Single(scenario.Rentals.Rentals).Status);
    }

    [Fact]
    public async Task Execute_DistinguishesInvalidAndMissingReferences()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();

        var invalid = await scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(Guid.Empty, vehicle.Id),
            CancellationToken.None);
        var missingPerson = await scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(Guid.NewGuid(), vehicle.Id),
            CancellationToken.None);
        var missingVehicle = await scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(person.Value, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(ReturnVehicleResultType.InvalidInput, invalid.Type);
        Assert.Equal(ReturnVehicleResultType.PersonNotFound, missingPerson.Type);
        Assert.Equal(ReturnVehicleResultType.VehicleNotFound, missingVehicle.Type);
    }
}
