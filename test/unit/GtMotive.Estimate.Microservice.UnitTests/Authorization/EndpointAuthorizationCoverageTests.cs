using System;
using System.Linq;
using System.Reflection;
using GtMotive.Estimate.Microservice.Api;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class EndpointAuthorizationCoverageTests
{
    [Fact]
    public void EveryBusinessActionHasOneCatalogedDeclarationAndIsNotAnonymous()
    {
        var actions = typeof(ApiConfiguration).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToArray();

        Assert.Equal(4, actions.Length);
        foreach (var action in actions)
        {
            Assert.False(action.IsDefined(typeof(AllowAnonymousAttribute), inherit: true));
            var declaration = Assert.Single(
                action.GetCustomAttributes<ApiAuthorizationAttribute>(inherit: true));
            Assert.True(AuthorizationCatalog.IsKnownResource(declaration.ResourceName));
            Assert.All(
                declaration.PolicyNames,
                policy => Assert.True(AuthorizationCatalog.IsKnownPolicy(policy)));
        }
    }
}
