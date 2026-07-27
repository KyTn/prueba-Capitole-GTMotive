namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;

public enum ReturnVehicleResultType
{
    Returned,
    InvalidInput,
    PersonNotFound,
    VehicleNotFound,
    VehicleNotRented,
    RentalOwnershipConflict,
    RentalAlreadyReturned,
}

public sealed record ReturnVehicleResult(
    ReturnVehicleResultType Type,
    RentalDto Rental,
    string Code,
    string Detail)
{
    public static ReturnVehicleResult Returned(RentalDto rental) =>
        new(ReturnVehicleResultType.Returned, rental, null, null);

    public static ReturnVehicleResult Failure(ReturnVehicleResultType type, string code, string detail) =>
        new(type, null, code, detail);
}
