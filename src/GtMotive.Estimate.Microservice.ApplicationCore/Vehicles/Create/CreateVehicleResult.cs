using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;

public enum CreateVehicleResultType
{
    Created,
    InvalidInput,
    VehicleTooOld,
    VehicleAlreadyExists,
}

public sealed record CreateVehicleResult(
    CreateVehicleResultType Type,
    VehicleDto Vehicle,
    string Code,
    string Detail)
{
    public static CreateVehicleResult Created(VehicleDto vehicle) =>
        new(CreateVehicleResultType.Created, vehicle, null, null);

    public static CreateVehicleResult Failure(CreateVehicleResultType type, string code, string detail) =>
        new(type, null, code, detail);
}
