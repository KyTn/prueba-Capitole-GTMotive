using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Vehicles;

public sealed class CreateVehicleUseCaseTests
{
    [Fact]
    public async Task Execute_CreatesVehicleWithoutHost()
    {
        var repository = new InMemoryVehicleRepository();
        var useCase = new CreateVehicleUseCase(
            repository,
            new FixedClock(new DateOnly(2026, 7, 27)),
            new NullAppLogger<CreateVehicleUseCase>());

        var result = await useCase.ExecuteAsync(
            new CreateVehicleCommand(" 1234abc ", "Toyota", "Corolla", new DateOnly(2024, 1, 1)),
            CancellationToken.None);

        Assert.Equal(CreateVehicleResultType.Created, result.Type);
        Assert.Equal("1234ABC", result.Vehicle.RegistrationNumber);
        Assert.Equal(1, repository.Count);
    }
}
