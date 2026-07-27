using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class PersonActiveRentalConflictTests
{
    [Fact]
    public async Task Execute_ConcurrentRequestsLeaveOneRentalForPerson()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var firstVehicle = await scenario.AddVehicleAsync();
        var secondVehicle = await scenario.AddVehicleAsync();

        var results = await Task.WhenAll(
            scenario.UseCase.ExecuteAsync(
                new RentVehicleCommand(person.Value, firstVehicle.Id),
                CancellationToken.None),
            scenario.UseCase.ExecuteAsync(
                new RentVehicleCommand(person.Value, secondVehicle.Id),
                CancellationToken.None));

        Assert.Single(results, result => result.Type == RentVehicleResultType.Created);
        Assert.Single(results, result => result.Type == RentVehicleResultType.PersonAlreadyHasActiveRental);
        Assert.Single(scenario.Rentals.Rentals);
    }
}
