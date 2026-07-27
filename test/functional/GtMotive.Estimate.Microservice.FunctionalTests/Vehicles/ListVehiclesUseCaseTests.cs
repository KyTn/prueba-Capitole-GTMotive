using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Vehicles;

public sealed class ListVehiclesUseCaseTests
{
    [Fact]
    public async Task Execute_ListsAllVehiclesWithoutHostAndDoesNotModifyRepository()
    {
        var repository = new InMemoryVehicleRepository();
        var first = CreateVehicle("1234ABC", "Toyota", "Corolla");
        var second = CreateVehicle("5678DEF", "Seat", "Leon");
        await repository.AddAsync(first, CancellationToken.None);
        await repository.AddAsync(second, CancellationToken.None);
        var useCase = new ListVehiclesUseCase(
            repository,
            new NullAppLogger<ListVehiclesUseCase>());

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(2, result.Vehicles.Count);
        Assert.Equal(2, repository.Count);
        Assert.Contains(result.Vehicles, vehicle => vehicle.Id == first.Id);
        Assert.Contains(result.Vehicles, vehicle => vehicle.Id == second.Id);
        Assert.Equal(2, new HashSet<Guid> { result.Vehicles[0].Id, result.Vehicles[1].Id }.Count);
    }

    [Fact]
    public async Task Execute_WhenReadFails_DoesNotReturnPartialResult()
    {
        var useCase = new ListVehiclesUseCase(
            new FailingVehicleRepository(),
            new NullAppLogger<ListVehiclesUseCase>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(CancellationToken.None));
    }

    private static Vehicle CreateVehicle(string registrationNumber, string brand, string model) =>
        Vehicle.Rehydrate(Guid.NewGuid(), registrationNumber, brand, model, new DateOnly(2024, 1, 1));

    private sealed class FailingVehicleRepository : IVehicleRepository
    {
        public Task<bool> ExistsByRegistrationNumberAsync(
            RegistrationNumber registrationNumber,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Read failed.");

        public Task<Vehicle> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
