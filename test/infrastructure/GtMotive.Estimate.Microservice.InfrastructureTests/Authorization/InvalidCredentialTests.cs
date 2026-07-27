using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class InvalidCredentialTests
{
    [Fact]
    public async Task RejectionDoesNotExposeCredentialOrClaims()
    {
        await using var factory = new AuthorizationApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/vehicles");
        request.Headers.Add(TestAuthenticationHandler.AnonymousHeader, "expired-secret-token");

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("expired-secret-token", body);
        Assert.DoesNotContain(AuthorizationCatalog.PermissionClaimType, body);
    }
}
