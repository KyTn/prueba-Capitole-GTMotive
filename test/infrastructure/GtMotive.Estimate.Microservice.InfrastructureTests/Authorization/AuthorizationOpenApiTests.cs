using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class AuthorizationOpenApiTests
{
    [Fact]
    public void OpenApiDocumentsSecurityAndAuthorizationResponses()
    {
        using var factory = new AuthorizationApiFactory();
        var options = factory.Services
            .GetRequiredService<IOptions<SwaggerGeneratorOptions>>()
            .Value;
        var documentName = Assert.Single(options.SwaggerDocs.Keys);
        var provider = factory.Services.GetRequiredService<ISwaggerProvider>();

        var document = provider.GetSwagger(documentName);

        AssertProtected(document.Paths["/vehicles"].Operations.Single(
            pair => pair.Key.ToString() == "Get").Value);
        AssertProtected(document.Paths["/vehicles"].Operations.Single(
            pair => pair.Key.ToString() == "Post").Value);
        AssertProtected(document.Paths["/rentals"].Operations.Single(
            pair => pair.Key.ToString() == "Post").Value);
        AssertProtected(document.Paths["/rentals/returns"].Operations.Single(
            pair => pair.Key.ToString() == "Post").Value);
    }

    private static void AssertProtected(Microsoft.OpenApi.Models.OpenApiOperation operation)
    {
        Assert.NotEmpty(operation.Security);
        Assert.Contains("401", operation.Responses.Keys);
        Assert.Contains("403", operation.Responses.Keys);
    }
}
