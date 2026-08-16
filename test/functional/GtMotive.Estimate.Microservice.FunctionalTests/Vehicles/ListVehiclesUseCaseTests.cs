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
