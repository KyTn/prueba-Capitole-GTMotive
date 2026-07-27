using System.Linq;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Authorization;

public sealed class AuthorizationPipelineTests
{
    [Fact]
    public void AuthorizationFilterCannotReachApplicationOrSuccessSideEffects()
    {
        var dependencies = typeof(ApiAuthorizationHandler)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        Assert.Contains(typeof(IAuthorizationService), dependencies);
        Assert.Single(dependencies);
        Assert.DoesNotContain(
            dependencies,
            type => type.Namespace?.Contains("MediatR", System.StringComparison.Ordinal) == true);
        Assert.DoesNotContain(typeof(IBusFactory), dependencies);
        Assert.DoesNotContain(typeof(ITelemetry), dependencies);
    }
}
