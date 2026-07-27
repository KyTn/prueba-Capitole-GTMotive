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
