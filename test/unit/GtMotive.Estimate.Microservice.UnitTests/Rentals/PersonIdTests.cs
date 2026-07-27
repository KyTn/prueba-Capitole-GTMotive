using System;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Rentals;

public sealed class PersonIdTests
{
    [Fact]
    public void Constructor_RejectsEmptyId()
    {
        var exception = Assert.Throws<RentalValidationException>(() => new PersonId(Guid.Empty));

        Assert.Equal("invalid_person_id", exception.Code);
    }
}
