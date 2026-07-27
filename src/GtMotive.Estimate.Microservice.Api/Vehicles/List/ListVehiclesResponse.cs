using System;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.List;

public sealed record ListVehiclesResponse(
    Guid Id,
    string RegistrationNumber,
    string Brand,
    string Model,
    DateOnly ManufactureDate);
