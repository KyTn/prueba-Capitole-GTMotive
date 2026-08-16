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

public sealed class RentVehicleEndpointTests
{
    [Fact]
    public async Task Post_CreatesRentalAndRejectsOccupiedVehicle()
    {
        await using var factory = new RentalApiFactory();
        var firstPerson = factory.AddPerson();
        var secondPerson = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = firstPerson.Value, vehicleId = vehicle.Id });
        var conflict = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = secondPerson.Value, vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<RentalResponse>();
        Assert.Equal(firstPerson.Value, body.PersonId);
        Assert.Equal(vehicle.Id, body.VehicleId);
        Assert.Single(factory.Rentals.Rentals);
    }

    private sealed record RentalResponse(
        Guid Id,
        Guid PersonId,
        Guid VehicleId,
        DateTimeOffset StartedAt,
        string Status);
}
