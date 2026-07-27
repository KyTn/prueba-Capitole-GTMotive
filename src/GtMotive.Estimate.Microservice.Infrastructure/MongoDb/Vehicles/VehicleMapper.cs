using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Vehicles;

internal static class VehicleMapper
{
    public static VehicleDocument ToDocument(Vehicle vehicle) =>
        new()
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber.Value,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            ManufactureDate = vehicle.ManufactureDate.ToDateTime(System.TimeOnly.MinValue),
        };

    public static Vehicle ToDomain(VehicleDocument document) =>
        Vehicle.Rehydrate(
            document.Id,
            document.RegistrationNumber,
            document.Brand,
            document.Model,
            System.DateOnly.FromDateTime(document.ManufactureDate));
}
