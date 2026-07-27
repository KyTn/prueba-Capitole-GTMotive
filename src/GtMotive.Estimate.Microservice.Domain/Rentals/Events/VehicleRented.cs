using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals.Events;

public sealed record VehicleRented(
    Guid RentalId,
    Guid VehicleId,
    Guid PersonId,
    DateTimeOffset StartedAt);
