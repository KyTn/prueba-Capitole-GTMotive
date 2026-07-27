using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;

public sealed record CreateVehicleCommand(
    string RegistrationNumber,
    string Brand,
    string Model,
    DateOnly ManufactureDate);
