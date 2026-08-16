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
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class RentVehicleNotFoundTests
{
    [Fact]
    public async Task Execute_UnknownPersonDoesNotCreateRental()
    {
        var scenario = new RentalScenario();
        var vehicle = await scenario.AddVehicleAsync();

        var result = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(Guid.NewGuid(), vehicle.Id),
            CancellationToken.None);

        Assert.Equal(RentVehicleResultType.PersonNotFound, result.Type);
        Assert.Empty(scenario.Rentals.Rentals);
    }

    [Fact]
    public async Task Execute_UnknownVehicleDoesNotCreateRental()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();

        var result = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(person.Value, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(RentVehicleResultType.VehicleNotFound, result.Type);
        Assert.Empty(scenario.Rentals.Rentals);
    }
}
