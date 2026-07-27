using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Return;

[ApiController]
[Route("rentals/returns")]
public sealed class RentalReturnsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ApiAuthorization(
        AuthorizationCatalog.Resources.Rentals,
        AuthorizationCatalog.Policies.RentalsReturn)]
    [ProducesResponseType(typeof(ReturnVehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReturnAsync(
        [FromBody] ReturnVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ReturnVehicleCommand(request.PersonId, request.VehicleId),
            cancellationToken);
        return ReturnVehiclePresenter.Present(result);
    }
}
