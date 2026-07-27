using System;
using System.ComponentModel.DataAnnotations;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Return;

public sealed record ReturnVehicleRequest(
    [Required] Guid PersonId,
    [Required] Guid VehicleId);
