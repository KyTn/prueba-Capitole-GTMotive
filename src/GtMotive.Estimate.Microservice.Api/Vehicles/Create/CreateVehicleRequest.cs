using System;
using System.ComponentModel.DataAnnotations;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.Create;

public sealed class CreateVehicleRequest
{
    [Required]
    public string RegistrationNumber { get; set; }

    [Required]
    public string Brand { get; set; }

    [Required]
    public string Model { get; set; }

    public DateOnly ManufactureDate { get; set; }
}
