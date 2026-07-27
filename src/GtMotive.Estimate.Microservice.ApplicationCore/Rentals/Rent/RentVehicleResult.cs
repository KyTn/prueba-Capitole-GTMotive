using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;

public enum RentVehicleResultType
{
    Created,
    InvalidInput,
    PersonNotFound,
    VehicleNotFound,
    PersonAlreadyHasActiveRental,
    VehicleNotAvailable,
}

public sealed record RentVehicleResult(
    RentVehicleResultType Type,
    RentalDto Rental,
    string Code,
    string Detail) : IUseCaseOutput
{
    public static RentVehicleResult Created(RentalDto rental) =>
        new(RentVehicleResultType.Created, rental, null, null);

    public static RentVehicleResult Failure(RentVehicleResultType type, string code, string detail) =>
        new(type, null, code, detail);
}
