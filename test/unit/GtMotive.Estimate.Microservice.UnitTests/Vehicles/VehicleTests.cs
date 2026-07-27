using System;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Vehicles;

public sealed class VehicleTests
{
    private static readonly DateOnly RegistrationDate = new(2026, 7, 27);

    [Fact]
    public void Create_NormalizesRegistrationNumberAndText()
    {
        var vehicle = Vehicle.Create(
            Guid.NewGuid(), " 1234abc ", " Toyota ", " Corolla ", new DateOnly(2024, 1, 1), RegistrationDate);

        Assert.Equal("1234ABC", vehicle.RegistrationNumber.Value);
        Assert.Equal("Toyota", vehicle.Brand);
        Assert.Equal("Corolla", vehicle.Model);
    }

    [Fact]
    public void Create_AcceptsExactlyFiveYears()
    {
        var vehicle = Vehicle.Create(
            Guid.NewGuid(), "1234ABC", "Toyota", "Corolla", RegistrationDate.AddYears(-5), RegistrationDate);

        Assert.Equal(RegistrationDate.AddYears(-5), vehicle.ManufactureDate);
    }

    [Fact]
    public void Create_RejectsOneDayOlderThanFiveYears()
    {
        var exception = Assert.Throws<VehicleValidationException>(() => Vehicle.Create(
            Guid.NewGuid(), "1234ABC", "Toyota", "Corolla", RegistrationDate.AddYears(-5).AddDays(-1), RegistrationDate));

        Assert.Equal(VehicleErrorCodes.VehicleTooOld, exception.Code);
    }

    [Fact]
    public void Create_RejectsFutureDate()
    {
        var exception = Assert.Throws<VehicleValidationException>(() => Vehicle.Create(
            Guid.NewGuid(), "1234ABC", "Toyota", "Corolla", RegistrationDate.AddDays(1), RegistrationDate));

        Assert.Equal(VehicleErrorCodes.FutureManufactureDate, exception.Code);
    }

    [Fact]
    public void Create_UsesLastFebruaryDayForLeapYearBoundary()
    {
        var registrationDate = new DateOnly(2024, 2, 29);
        var vehicle = Vehicle.Create(
            Guid.NewGuid(), "1234ABC", "Toyota", "Corolla", new DateOnly(2019, 2, 28), registrationDate);

        Assert.Equal(new DateOnly(2019, 2, 28), vehicle.ManufactureDate);
    }
}
