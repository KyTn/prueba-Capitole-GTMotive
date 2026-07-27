using System;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class ApiAuthorizationAttributeTests
{
    [Fact]
    public void ConstructorNormalizesAndDeduplicatesPolicies()
    {
        var attribute = new ApiAuthorizationAttribute(
            " Vehicles ",
            " Vehicles.Read ",
            "Vehicles.Read",
            "Vehicles.Create");

        Assert.Equal("Vehicles", attribute.ResourceName);
        Assert.Equal(["Vehicles.Read", "Vehicles.Create"], attribute.PolicyNames);
        var requirement = Assert.IsType<ApiAuthorizationRequirement>(
            Assert.Single(attribute.GetRequirements()));
        Assert.Equal(attribute.ResourceName, requirement.ResourceName);
        Assert.Equal(attribute.PolicyNames, requirement.PolicyNames);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsInvalidResource(string resource) =>
        Assert.ThrowsAny<ArgumentException>(
            () => new ApiAuthorizationAttribute(resource, "Vehicles.Read"));

    [Fact]
    public void ConstructorRejectsEmptyPolicies() =>
        Assert.Throws<ArgumentException>(
            () => new ApiAuthorizationAttribute("Vehicles", []));
}
