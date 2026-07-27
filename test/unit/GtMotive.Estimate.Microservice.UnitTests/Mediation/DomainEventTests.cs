using System;
using System.Linq;
using GtMotive.Estimate.Microservice.Domain.Rentals.Events;
using GtMotive.Estimate.Microservice.Domain.Vehicles.Events;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Mediation;

public sealed class DomainEventTests
{
    [Fact]
    public void EventsExposeOnlyRequiredImmutableData()
    {
        var rentalId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Equal(vehicleId, new VehicleCreated(vehicleId).VehicleId);
        Assert.Equal(rentalId, new VehicleRented(rentalId, vehicleId, personId, occurredAt).RentalId);
        Assert.Equal(occurredAt, new VehicleReturned(rentalId, vehicleId, personId, occurredAt).EndedAt);
        Assert.True(typeof(VehicleCreated).IsSealed);
        Assert.True(typeof(VehicleRented).IsSealed);
        Assert.True(typeof(VehicleReturned).IsSealed);
        Assert.Equal(new[] { "VehicleId" }, PropertyNames<VehicleCreated>());
        Assert.Equal(
            new[] { "PersonId", "RentalId", "StartedAt", "VehicleId" },
            PropertyNames<VehicleRented>());
        Assert.Equal(
            new[] { "EndedAt", "PersonId", "RentalId", "VehicleId" },
            PropertyNames<VehicleReturned>());
    }

    private static string[] PropertyNames<T>() =>
        typeof(T).GetProperties().Select(property => property.Name).OrderBy(name => name).ToArray();
}
