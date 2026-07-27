using System.Linq;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.List;

internal static class ListVehiclesPresenter
{
    public static IActionResult Present(ControllerBase controller, ListVehiclesResult result)
    {
        var response = result.Vehicles
            .Select(vehicle => new ListVehiclesResponse(
                vehicle.Id,
                vehicle.RegistrationNumber,
                vehicle.Brand,
                vehicle.Model,
                vehicle.ManufactureDate))
            .ToArray();

        return controller.Ok(response);
    }
}
