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

public sealed class PersonActiveRentalConflictTests
{
    [Fact]
    public async Task Execute_ConcurrentRequestsLeaveOneRentalForPerson()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var firstVehicle = await scenario.AddVehicleAsync();
        var secondVehicle = await scenario.AddVehicleAsync();

        var results = await Task.WhenAll(
            scenario.UseCase.ExecuteAsync(
                new RentVehicleCommand(person.Value, firstVehicle.Id),
                CancellationToken.None),
            scenario.UseCase.ExecuteAsync(
                new RentVehicleCommand(person.Value, secondVehicle.Id),
                CancellationToken.None));

        Assert.Single(results, result => result.Type == RentVehicleResultType.Created);
        Assert.Single(results, result => result.Type == RentVehicleResultType.PersonAlreadyHasActiveRental);
        Assert.Single(scenario.Rentals.Rentals);
    }
}
