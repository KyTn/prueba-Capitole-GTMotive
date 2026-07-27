using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Vehicles;

public sealed class CreateVehicleConflictTests
{
    [Fact]
    public async Task Execute_RejectsNormalizedDuplicate()
    {
        var repository = new InMemoryVehicleRepository();
        var useCase = new CreateVehicleUseCase(
            repository,
            new FixedClock(new DateOnly(2026, 7, 27)),
            new NullAppLogger<CreateVehicleUseCase>());
        var date = new DateOnly(2024, 1, 1);

        await useCase.ExecuteAsync(
            new CreateVehicleCommand("1234abc", "Toyota", "Corolla", date),
            CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync(
            new CreateVehicleCommand(" 1234ABC ", "Ford", "Focus", date),
            CancellationToken.None);

        Assert.Equal(CreateVehicleResultType.VehicleAlreadyExists, duplicate.Type);
        Assert.Equal(1, repository.Count);
    }
}
