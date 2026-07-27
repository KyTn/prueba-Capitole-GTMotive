using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class ReturnVehicleUseCaseTests
{
    [Fact]
    public async Task Execute_ClosesRentalAndReleasesPersonAndVehicle()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();
        var firstRent = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);

        var returned = await scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);
        var secondRent = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);

        Assert.Equal(ReturnVehicleResultType.Returned, returned.Type);
        Assert.Equal(RentalStatus.Closed, scenario.Rentals.Rentals.First().Status);
        Assert.NotNull(scenario.Rentals.Rentals.First().EndedAt);
        Assert.Equal(RentVehicleResultType.Created, firstRent.Type);
        Assert.Equal(RentVehicleResultType.Created, secondRent.Type);
    }
}
