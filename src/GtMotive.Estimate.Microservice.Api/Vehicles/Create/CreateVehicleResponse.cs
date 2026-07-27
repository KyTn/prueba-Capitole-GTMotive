using System;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.Create;

public sealed record CreateVehicleResponse(
    Guid Id,
    string RegistrationNumber,
    string Brand,
    string Model,
    DateOnly ManufactureDate);
