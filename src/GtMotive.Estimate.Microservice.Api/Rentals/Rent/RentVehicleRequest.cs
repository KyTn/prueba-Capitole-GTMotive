using System;
using System.ComponentModel.DataAnnotations;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Rent;

public sealed record RentVehicleRequest(
    [Required] Guid PersonId,
    [Required] Guid VehicleId);
