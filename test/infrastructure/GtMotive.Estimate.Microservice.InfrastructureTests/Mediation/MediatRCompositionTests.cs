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
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;
using GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Mediation;

public sealed class MediatRCompositionTests
{
    [Fact]
    public void HostResolvesExactlyOneHandlerPerMessageAndInfrastructurePorts()
    {
        using var factory = new RentalApiFactory();
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        Assert.Single(services.GetServices<IRequestHandler<CreateVehicleCommand, CreateVehicleResult>>());
        Assert.Single(services.GetServices<IRequestHandler<ListVehiclesQuery, ListVehiclesResult>>());
        Assert.Single(services.GetServices<IRequestHandler<RentVehicleCommand, RentVehicleResult>>());
        Assert.Single(services.GetServices<IRequestHandler<ReturnVehicleCommand, ReturnVehicleResult>>());
        Assert.NotNull(services.GetRequiredService<IBusFactory>());
        Assert.NotNull(services.GetRequiredService<ITelemetry>());
    }
}
