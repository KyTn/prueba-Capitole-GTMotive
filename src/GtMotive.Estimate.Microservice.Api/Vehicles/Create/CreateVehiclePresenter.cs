using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.Create;

internal static class CreateVehiclePresenter
{
    public static IActionResult Present(ControllerBase controller, CreateVehicleResult result)
    {
        if (result.Type == CreateVehicleResultType.Created)
        {
            var vehicle = result.Vehicle;
            return controller.Created(
                $"/vehicles/{vehicle.Id}",
                new CreateVehicleResponse(
                    vehicle.Id,
                    vehicle.RegistrationNumber,
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.ManufactureDate));
        }

        var status = result.Type switch
        {
            CreateVehicleResultType.VehicleTooOld => StatusCodes.Status422UnprocessableEntity,
            CreateVehicleResultType.VehicleAlreadyExists => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };
        var problem = new ProblemDetails
        {
            Status = status,
            Title = result.Type.ToString(),
            Detail = result.Detail,
            Type = $"https://httpstatuses.com/{status}",
        };
        problem.Extensions["code"] = result.Code;
        return new ObjectResult(problem) { StatusCode = status };
    }
}
