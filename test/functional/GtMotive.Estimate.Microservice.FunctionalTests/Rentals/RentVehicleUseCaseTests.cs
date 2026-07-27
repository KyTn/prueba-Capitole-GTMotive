using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class RentVehicleUseCaseTests
{
    [Fact]
    public async Task Execute_CreatesRentalAndRejectsOccupiedVehicle()
    {
        var scenario = new RentalScenario();
        var firstPerson = scenario.AddPerson();
        var secondPerson = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();

        var created = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(firstPerson.Value, vehicle.Id),
            CancellationToken.None);
        var conflict = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(secondPerson.Value, vehicle.Id),
            CancellationToken.None);

        Assert.Equal(RentVehicleResultType.Created, created.Type);
        Assert.Equal(RentVehicleResultType.VehicleNotAvailable, conflict.Type);
        Assert.Single(scenario.Rentals.Rentals);
        Assert.Equal(firstPerson, scenario.Rentals.Rentals.Single().PersonId);
    }
}
