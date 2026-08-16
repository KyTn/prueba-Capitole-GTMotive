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

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;

public sealed class CreateVehicleEndpointTests
{
    [Fact]
    public async Task Post_CreatesVehicleThroughHost()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber = " 1234abc ",
                brand = "Toyota",
                model = "Corolla",
                manufactureDate = new DateOnly(2024, 1, 1),
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal(1, factory.Repository.Count);
        var body = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.Equal("1234ABC", body.RegistrationNumber);
    }

    private sealed record VehicleResponse(
        Guid Id,
        string RegistrationNumber,
        string Brand,
        string Model,
        DateOnly ManufactureDate);
}
