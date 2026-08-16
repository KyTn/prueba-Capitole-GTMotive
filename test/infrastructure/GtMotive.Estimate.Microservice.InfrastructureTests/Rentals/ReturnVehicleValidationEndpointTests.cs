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
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class ReturnVehicleValidationEndpointTests
{
    [Fact]
    public async Task Post_WrongOwnerReturnsConflictAndKeepsRentalActive()
    {
        await using var factory = new RentalApiFactory();
        var owner = factory.AddPerson();
        var other = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        await factory.AddActiveRentalAsync(owner, vehicle);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = other.Value, VehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(RentalStatus.Active, Assert.Single(factory.Rentals.Rentals).Status);
    }

    [Fact]
    public async Task Post_EmptyPersonReturnsBadRequest()
    {
        await using var factory = new RentalApiFactory();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = Guid.Empty, VehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_UnknownReferencesReturnNotFound()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var unknownPerson = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = Guid.NewGuid(), VehicleId = vehicle.Id });
        var unknownVehicle = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = person.Value, VehicleId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, unknownPerson.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknownVehicle.StatusCode);
    }
}
