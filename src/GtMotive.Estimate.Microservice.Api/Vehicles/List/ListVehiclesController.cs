using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.List;

[ApiController]
[Route("vehicles")]
public sealed class ListVehiclesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ApiAuthorization(
        AuthorizationCatalog.Resources.Vehicles,
        AuthorizationCatalog.Policies.VehiclesRead)]
    [ProducesResponseType(typeof(IReadOnlyList<ListVehiclesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListVehiclesQuery(), cancellationToken);
        return ListVehiclesPresenter.Present(this, result);
    }
}
