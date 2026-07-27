using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;

public sealed record VehicleDto(
    Guid Id,
    string RegistrationNumber,
    string Brand,
    string Model,
    DateOnly ManufactureDate);
