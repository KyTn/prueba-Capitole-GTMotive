using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class ProtectedEndpointsTests
{
    public static TheoryData<HttpMethod, string> Endpoints => new()
    {
        { HttpMethod.Post, "/vehicles" },
        { HttpMethod.Get, "/vehicles" },
        { HttpMethod.Post, "/rentals" },
        { HttpMethod.Post, "/rentals/returns" }
    };

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task EndpointChallengesAnonymousRequest(HttpMethod method, string path)
    {
        await using var factory = new AuthorizationApiFactory();
        using var client = factory.CreateClient();
        using var request = CreateRequest(method, path);
        request.Headers.Add(TestAuthenticationHandler.AnonymousHeader, "true");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task EndpointForbidsPrincipalWithoutPermission(HttpMethod method, string path)
    {
        await using var factory = new AuthorizationApiFactory();
        using var client = factory.CreateClient();
        using var request = CreateRequest(method, path);
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, "Unknown.Permission");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string path) =>
        new(method, path)
        {
            Content = method == HttpMethod.Get
                ? null
                : new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };
}

