using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;

public sealed record RentVehicleCommand(Guid PersonId, Guid VehicleId);
