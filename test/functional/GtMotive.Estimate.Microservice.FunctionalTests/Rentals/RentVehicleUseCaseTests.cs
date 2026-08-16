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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Rentals;

public sealed class RentVehicleUseCaseTests
{
    [Fact]
    public async Task Execute_CreatesRentalAndRejectsOccupiedVehicle()
    {
        var scenario = new RentalScenario();
        var firstPerson = scenario.AddPerson();
        var secondPerson = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();

        var created = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(firstPerson.Value, vehicle.Id),
            CancellationToken.None);
        var conflict = await scenario.UseCase.ExecuteAsync(
            new RentVehicleCommand(secondPerson.Value, vehicle.Id),
            CancellationToken.None);

        Assert.Equal(RentVehicleResultType.Created, created.Type);
        Assert.Equal(RentVehicleResultType.VehicleNotAvailable, conflict.Type);
        Assert.Single(scenario.Rentals.Rentals);
        Assert.Equal(firstPerson, scenario.Rentals.Rentals.Single().PersonId);
    }
}
