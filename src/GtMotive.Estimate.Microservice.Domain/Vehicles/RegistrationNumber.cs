using System;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles;

public sealed record RegistrationNumber
{
    public RegistrationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new VehicleValidationException(VehicleErrorCodes.InvalidVehicle, "Registration number is required.");
        }

        Value = value.Trim().ToUpperInvariant();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
