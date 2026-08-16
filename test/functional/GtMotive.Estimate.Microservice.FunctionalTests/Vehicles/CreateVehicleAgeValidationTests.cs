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
