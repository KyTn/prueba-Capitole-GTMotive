using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Vehicles;

public sealed class ListVehiclesUseCaseTests
{
    [Fact]
    public async Task Execute_ReturnsEveryVehicleExactlyOnce()
    {
        var vehicles = new[]
        {
            Vehicle.Rehydrate(
                Guid.Parse("4d7e9e41-bd80-4b71-b13a-bf20212c4ac7"),
                "1234abc",
                "Toyota",
                "Corolla",
                new DateOnly(2024, 1, 1)),
            Vehicle.Rehydrate(
                Guid.Parse("d3fc2eb4-dc47-4d64-81cc-293a2d4748c3"),
                "5678def",
                "Seat",
                "Leon",
                new DateOnly(2023, 6, 15)),
        };
        var repository = new StubVehicleRepository(vehicles);
        var useCase = new ListVehiclesUseCase(repository, new NullLogger<ListVehiclesUseCase>());

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(2, result.Vehicles.Count);
        Assert.Collection(
            result.Vehicles,
            vehicle =>
            {
                Assert.Equal(vehicles[0].Id, vehicle.Id);
                Assert.Equal("1234ABC", vehicle.RegistrationNumber);
                Assert.Equal("Toyota", vehicle.Brand);
                Assert.Equal("Corolla", vehicle.Model);
                Assert.Equal(new DateOnly(2024, 1, 1), vehicle.ManufactureDate);
            },
            vehicle => Assert.Equal(vehicles[1].Id, vehicle.Id));
        Assert.Equal(2, repository.Count);
    }

    [Fact]
    public async Task Execute_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var useCase = new ListVehiclesUseCase(
            new StubVehicleRepository(Array.Empty<Vehicle>()),
            new NullLogger<ListVehiclesUseCase>());

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => useCase.ExecuteAsync(cancellation.Token));
    }

    private sealed class StubVehicleRepository(IReadOnlyList<Vehicle> vehicles) : IVehicleRepository
    {
        public int Count => vehicles.Count;

        public Task<bool> ExistsByRegistrationNumberAsync(
            RegistrationNumber registrationNumber,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(vehicles);
        }

        public Task<Vehicle> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class NullLogger<T> : IAppLogger<T>
    {
        public void LogInformation(string message, params object[] args) { }

        public void LogWarning(string message, params object[] args) { }

        public void LogError(Exception exception, string message, params object[] args) { }

        public void LogDebug(string message, params object[] args) { }
    }
}
