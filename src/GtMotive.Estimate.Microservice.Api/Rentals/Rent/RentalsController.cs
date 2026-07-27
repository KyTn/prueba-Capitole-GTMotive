using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Rent;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Rent;

[ApiController]
[Route("rentals")]
public sealed class RentalsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ApiAuthorization(
        AuthorizationCatalog.Resources.Rentals,
        AuthorizationCatalog.Policies.RentalsCreate)]
    [ProducesResponseType(typeof(RentVehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RentAsync(
        [FromBody] RentVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RentVehicleCommand(request.PersonId, request.VehicleId),
            cancellationToken);
        return RentVehiclePresenter.Present(this, result);
    }
}
