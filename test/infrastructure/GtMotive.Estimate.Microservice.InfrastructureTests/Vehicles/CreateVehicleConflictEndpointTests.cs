using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;

public sealed class CreateVehicleConflictEndpointTests
{
    [Fact]
    public async Task Post_ConcurrentDuplicatesLeaveOneVehicle()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();
        var request = new
        {
            registrationNumber = "9999xyz",
            brand = "Toyota",
            model = "Corolla",
            manufactureDate = new DateOnly(2024, 1, 1),
        };

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/vehicles", request),
            client.PostAsJsonAsync("/vehicles", request));

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal(1, factory.Repository.Count);
    }

    [Fact]
    public async Task Post_MissingRequiredFieldReturnsBadRequest()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber = "1234ABC",
                brand = string.Empty,
                model = "Corolla",
                manufactureDate = new DateOnly(2024, 1, 1),
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, factory.Repository.Count);
    }
}
