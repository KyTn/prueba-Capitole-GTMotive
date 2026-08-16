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
using GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;
using GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class AuthorizedEndpointCompatibilityTests
{
    [Fact]
    public async Task AuthorizedVehicleEndpointsReachExistingContracts()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        using var list = await client.GetAsync("/vehicles");
        using var create = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber = "1234ABC",
                brand = "Toyota",
                model = "Corolla",
                manufactureDate = new DateOnly(2024, 1, 1)
            });

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
    }

    [Fact]
    public async Task AuthorizedRentalEndpointsReachExistingContracts()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        using var rent = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = person.Value, vehicleId = vehicle.Id });
        using var returned = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { personId = person.Value, vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Created, rent.StatusCode);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
    }
}

