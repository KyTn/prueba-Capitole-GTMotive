using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals.Events;

public sealed record VehicleReturned(
    Guid RentalId,
    Guid VehicleId,
    Guid PersonId,
    DateTimeOffset EndedAt);
