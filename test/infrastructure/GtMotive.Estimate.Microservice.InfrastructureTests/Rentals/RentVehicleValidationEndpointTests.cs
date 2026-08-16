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
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class RentVehicleValidationEndpointTests
{
    [Fact]
    public async Task Post_EmptyPersonIdReturnsBadRequest()
    {
        await using var factory = new RentalApiFactory();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = Guid.Empty, vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(factory.Rentals.Rentals);
    }

    [Fact]
    public async Task Post_UnknownPersonReturnsNotFound()
    {
        await using var factory = new RentalApiFactory();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = Guid.NewGuid(), vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(factory.Rentals.Rentals);
    }

    [Fact]
    public async Task Post_UnknownVehicleReturnsNotFound()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = person.Value, vehicleId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(factory.Rentals.Rentals);
    }
}
