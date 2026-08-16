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
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;

public sealed class ListVehiclesEndpointTests
{
    [Fact]
    public async Task Get_ReturnsEveryVehicleThroughHost()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();
        await CreateVehicleAsync(client, "1234abc", "Toyota", "Corolla");
        await CreateVehicleAsync(client, "5678def", "Seat", "Leon");

        var response = await client.GetAsync("/vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var vehicles = await response.Content.ReadFromJsonAsync<List<VehicleResponse>>();
        Assert.NotNull(vehicles);
        Assert.Equal(2, vehicles.Count);
        Assert.Contains(vehicles, vehicle =>
            vehicle.RegistrationNumber == "1234ABC" &&
            vehicle.Brand == "Toyota" &&
            vehicle.Model == "Corolla" &&
            vehicle.ManufactureDate == new DateOnly(2024, 1, 1) &&
            vehicle.Id != Guid.Empty);
        Assert.Equal(2, new HashSet<Guid> { vehicles[0].Id, vehicles[1].Id }.Count);
    }

    private static async Task CreateVehicleAsync(
        System.Net.Http.HttpClient client,
        string registrationNumber,
        string brand,
        string model)
    {
        var response = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber,
                brand,
                model,
                manufactureDate = new DateOnly(2024, 1, 1),
            });
        response.EnsureSuccessStatusCode();
    }

    private sealed record VehicleResponse(
        Guid Id,
        string RegistrationNumber,
        string Brand,
        string Model,
        DateOnly ManufactureDate);
}
