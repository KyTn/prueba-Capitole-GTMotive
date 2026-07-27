using System;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Vehicles;

public sealed class VehicleRequiredFieldsTests
{
    public static TheoryData<Guid, string, string, string> InvalidFields =>
        new()
        {
            { Guid.Empty, "1234ABC", "Toyota", "Corolla" },
            { Guid.NewGuid(), " ", "Toyota", "Corolla" },
            { Guid.NewGuid(), "1234ABC", " ", "Corolla" },
            { Guid.NewGuid(), "1234ABC", "Toyota", " " },
        };

    [Theory]
    [MemberData(nameof(InvalidFields))]
    public void Create_RejectsMissingFields(Guid id, string registration, string brand, string model)
    {
        var exception = Assert.Throws<VehicleValidationException>(() => Vehicle.Create(
            id, registration, brand, model, new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 1)));

        Assert.Equal(VehicleErrorCodes.InvalidVehicle, exception.Code);
    }
}
