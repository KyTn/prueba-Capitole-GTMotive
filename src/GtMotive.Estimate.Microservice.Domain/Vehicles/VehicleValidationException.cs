namespace GtMotive.Estimate.Microservice.Domain.Vehicles;

public static class VehicleErrorCodes
{
    public const string InvalidVehicle = "invalid_vehicle";
    public const string FutureManufactureDate = "future_manufacture_date";
    public const string VehicleTooOld = "vehicle_too_old";
}

public sealed class VehicleValidationException : DomainException
{
    public VehicleValidationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
