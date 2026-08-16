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
