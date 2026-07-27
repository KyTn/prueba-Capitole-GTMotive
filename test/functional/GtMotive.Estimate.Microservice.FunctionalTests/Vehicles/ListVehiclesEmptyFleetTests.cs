using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Vehicles;

public sealed class ListVehiclesEmptyFleetTests
{
    [Fact]
    public async Task Execute_ReturnsEmptyCollectionWithoutChangingRepository()
    {
        var repository = new InMemoryVehicleRepository();
        var useCase = new ListVehiclesUseCase(
            repository,
            new NullAppLogger<ListVehiclesUseCase>());

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.NotNull(result.Vehicles);
        Assert.Empty(result.Vehicles);
        Assert.Equal(0, repository.Count);
    }
}
