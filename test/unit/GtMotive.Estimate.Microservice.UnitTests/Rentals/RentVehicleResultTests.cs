using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Rentals;

public sealed class RentVehicleResultTests
{
    [Fact]
    public void PersonConflict_HasNoRental()
    {
        var result = RentVehicleResult.Failure(
            RentVehicleResultType.PersonAlreadyHasActiveRental,
            "person_already_has_active_rental",
            "Conflict");

        Assert.Null(result.Rental);
        Assert.Equal(RentVehicleResultType.PersonAlreadyHasActiveRental, result.Type);
    }
}
