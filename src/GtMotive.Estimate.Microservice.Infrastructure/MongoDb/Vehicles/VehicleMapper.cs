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
}
