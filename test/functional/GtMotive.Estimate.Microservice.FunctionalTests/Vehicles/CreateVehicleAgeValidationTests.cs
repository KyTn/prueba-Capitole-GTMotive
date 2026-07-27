using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Vehicles;

public sealed class CreateVehicleAgeValidationTests
{
    [Fact]
    public async Task Execute_RejectsOldVehicleWithoutPersisting()
    {
        var repository = new InMemoryVehicleRepository();
        var useCase = new CreateVehicleUseCase(
            repository,
            new FixedClock(new DateOnly(2026, 7, 27)),
            new NullAppLogger<CreateVehicleUseCase>());

        var result = await useCase.ExecuteAsync(
            new CreateVehicleCommand("1234ABC", "Toyota", "Corolla", new DateOnly(2021, 7, 26)),
            CancellationToken.None);

        Assert.Equal(CreateVehicleResultType.VehicleTooOld, result.Type);
        Assert.Equal(0, repository.Count);
    }
}
