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

public sealed class ListVehiclesEmptyFleetTests
{
    [Fact]
    public async Task Execute_ReturnsNonNullEmptyCollection()
    {
        var useCase = new ListVehiclesUseCase(
            new EmptyVehicleRepository(),
            new NullLogger<ListVehiclesUseCase>());

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.NotNull(result.Vehicles);
        Assert.Empty(result.Vehicles);
    }

    private sealed class EmptyVehicleRepository : IVehicleRepository
    {
        public Task<bool> ExistsByRegistrationNumberAsync(
            RegistrationNumber registrationNumber,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Vehicle>>(Array.Empty<Vehicle>());

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
