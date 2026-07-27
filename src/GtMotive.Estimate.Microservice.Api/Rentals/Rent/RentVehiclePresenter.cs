using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Rent;

internal static class RentVehiclePresenter
{
    public static IActionResult Present(ControllerBase controller, RentVehicleResult result)
    {
        if (result.Type == RentVehicleResultType.Created)
        {
            var rental = result.Rental;
            return controller.Created(
                $"/rentals/{rental.Id}",
                new RentVehicleResponse(
                    rental.Id,
                    rental.PersonId,
                    rental.VehicleId,
                    rental.StartedAt,
                    rental.Status));
        }

        var status = result.Type switch
        {
            RentVehicleResultType.PersonNotFound or RentVehicleResultType.VehicleNotFound =>
                StatusCodes.Status404NotFound,
            RentVehicleResultType.PersonAlreadyHasActiveRental or RentVehicleResultType.VehicleNotAvailable =>
                StatusCodes.Status409Conflict,
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
