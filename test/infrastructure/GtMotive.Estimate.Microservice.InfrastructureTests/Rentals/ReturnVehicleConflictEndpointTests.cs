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
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class ReturnVehicleConflictEndpointTests
{
    [Fact]
    public async Task Post_RejectsVehicleWithoutActiveRental()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = person.Value, VehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_ConcurrentReturnsCloseExactlyOnce()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        await factory.AddActiveRentalAsync(person, vehicle);
        using var client = factory.CreateClient();
        var request = new { PersonId = person.Value, VehicleId = vehicle.Id };

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/rentals/returns", request),
            client.PostAsJsonAsync("/rentals/returns", request));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Single(factory.Rentals.Rentals, rental => rental.EndedAt.HasValue);
    }
}
