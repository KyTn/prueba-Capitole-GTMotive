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
using GtMotive.Estimate.Microservice.Domain.Rentals.Events;
using GtMotive.Estimate.Microservice.FunctionalTests.Rentals;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Mediation;

public sealed class DomainEventPublishingTests
{
    [Fact]
    public async Task SuccessfulMutationPublishesOnceAndRejectedMutationDoesNotPublish()
    {
        var scenario = new RentalScenario();
        var person = scenario.AddPerson();
        var vehicle = await scenario.AddVehicleAsync();
        var busFactory = new RecordingBusFactory();
        var telemetry = new RecordingTelemetry();
        var handler = new RentVehicleHandler(scenario.UseCase, busFactory, telemetry);
        var request = new RentVehicleCommand(person.Value, vehicle.Id);

        var created = await handler.Handle(request, CancellationToken.None);
        var rejected = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(RentVehicleResultType.Created, created.Type);
        Assert.Equal(RentVehicleResultType.PersonAlreadyHasActiveRental, rejected.Type);
        Assert.Single(busFactory.Bus.Messages);
        Assert.IsType<VehicleRented>(busFactory.Bus.Messages.Single());
        Assert.Equal(2, telemetry.Events.Count);
        Assert.Contains(telemetry.Events, item => item.Properties["outcome"] == "success");
        Assert.Contains(telemetry.Events, item => item.Properties["outcome"] == "rejected");
        Assert.All(
            telemetry.Events,
            item => Assert.Equal(new[] { "operation", "outcome" }, item.Properties.Keys.OrderBy(key => key)));
    }
}
