using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Rent;

[ApiController]
[Route("rentals")]
public sealed class RentalsController(RentVehicleUseCase useCase) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RentVehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RentAsync(
        [FromBody] RentVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new RentVehicleCommand(request.PersonId, request.VehicleId),
            cancellationToken);
        return RentVehiclePresenter.Present(this, result);
    }
}
