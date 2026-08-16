/*
 * Aviso de propiedad intelectual
 *
 * Este repositorio se ha creado exclusivamente como prueba tÃ©cnica para Capitole.
 * Salvo los componentes de terceros y los derechos que pudieran haberse cedido
 * expresamente por contrato, el cÃ³digo y la documentaciÃ³n originales contenidos en
 * Ã©l son propiedad de su autor. No se autoriza su copia, reproducciÃ³n, modificaciÃ³n,
 * distribuciÃ³n, publicaciÃ³n ni explotaciÃ³n, total o parcial, sin consentimiento
 * previo y por escrito del titular de los derechos. El titular se reserva el
 * ejercicio de las acciones legales que correspondan frente a cualquier uso no
 * autorizado.
 */

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
