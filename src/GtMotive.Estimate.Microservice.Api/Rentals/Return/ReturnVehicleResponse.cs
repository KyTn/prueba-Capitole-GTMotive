using System;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Return;

public sealed record ReturnVehicleResponse(
    Guid Id,
    Guid PersonId,
    Guid VehicleId,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    string Status);
