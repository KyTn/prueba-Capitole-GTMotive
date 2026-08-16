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

public sealed class CreateVehicleValidationEndpointTests
{
    [Theory]
    [InlineData("2021-07-26", HttpStatusCode.UnprocessableEntity)]
    [InlineData("2026-07-28", HttpStatusCode.BadRequest)]
    public async Task Post_MapsDateFailures(string manufactureDate, HttpStatusCode status)
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber = Guid.NewGuid().ToString("N"),
                brand = "Toyota",
                model = "Corolla",
                manufactureDate = DateOnly.Parse(manufactureDate),
            });

        Assert.Equal(status, response.StatusCode);
        Assert.Equal(0, factory.Repository.Count);
    }
}
