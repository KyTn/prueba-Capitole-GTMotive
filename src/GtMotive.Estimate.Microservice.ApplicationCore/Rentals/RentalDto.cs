using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals;

public sealed record RentalDto(
    Guid Id,
    Guid PersonId,
    Guid VehicleId,
    DateTimeOffset StartedAt,
    string Status);
