using System;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Rent;

public sealed record RentVehicleResponse(
    Guid Id,
    Guid PersonId,
    Guid VehicleId,
    DateTimeOffset StartedAt,
    string Status);
