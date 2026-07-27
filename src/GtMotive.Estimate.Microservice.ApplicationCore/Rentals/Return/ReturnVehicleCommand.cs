using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;

public sealed record ReturnVehicleCommand(Guid PersonId, Guid VehicleId);
