using System;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Rentals;

public sealed class ReturnVehicleResultTests
{
    [Fact]
    public void Returned_PreservesRental()
    {
        var dto = new RentalDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-1),
            "closed",
            DateTimeOffset.UtcNow);

        var result = ReturnVehicleResult.Returned(dto);

        Assert.Equal(ReturnVehicleResultType.Returned, result.Type);
        Assert.Same(dto, result.Rental);
    }

    [Fact]
    public void Failure_PreservesStableCode()
    {
        var result = ReturnVehicleResult.Failure(
            ReturnVehicleResultType.VehicleNotRented,
            "vehicle_not_rented",
            "detail");

        Assert.Equal("vehicle_not_rented", result.Code);
        Assert.Null(result.Rental);
    }
}
