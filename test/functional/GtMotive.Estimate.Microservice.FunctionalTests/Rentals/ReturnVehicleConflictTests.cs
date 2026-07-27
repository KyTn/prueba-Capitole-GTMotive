using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class ReturnVehicleConflictTests
{
    [Fact]
    public async Task Execute_RejectsVehicleWithoutActiveRental()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();

        var result = await scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);

        Assert.Equal(ReturnVehicleResultType.VehicleNotRented, result.Type);
    }

    [Fact]
    public async Task Execute_ConcurrentReturnsCloseExactlyOnce()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();
        await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);

        var first = scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);
        var second = scenario.ReturnUseCase.ExecuteAsync(
            new ReturnVehicleCommand(person.Value, vehicle.Id),
            CancellationToken.None);
        var results = await Task.WhenAll(first, second);

        Assert.Single(results, result => result.Type == ReturnVehicleResultType.Returned);
        Assert.Single(results, result =>
            result.Type is ReturnVehicleResultType.VehicleNotRented or
                ReturnVehicleResultType.RentalAlreadyReturned);
        Assert.Single(scenario.Rentals.Rentals, rental => rental.EndedAt.HasValue);
    }
}
