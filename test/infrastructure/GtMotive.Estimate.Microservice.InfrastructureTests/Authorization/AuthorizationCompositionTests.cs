using GtMotive.Estimate.Microservice.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class AuthorizationCompositionTests
{
    [Fact]
    public void HostResolvesDomainAuthorizationAdapter()
    {
        using var factory = new AuthorizationApiFactory();
        using var scope = factory.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

        Assert.IsType<
            GtMotive.Estimate.Microservice.Api.Authorization.AuthorizationService>(service);
    }
}

