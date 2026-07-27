using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.Create;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Vehicles.Create;

[ApiController]
[Route("vehicles")]
public sealed class VehiclesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ApiAuthorization(
        AuthorizationCatalog.Resources.Vehicles,
        AuthorizationCatalog.Policies.VehiclesCreate)]
    [ProducesResponseType(typeof(CreateVehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateVehicleCommand(
                request.RegistrationNumber,
                request.Brand,
                request.Model,
                request.ManufactureDate),
            cancellationToken);
        return CreateVehiclePresenter.Present(this, result);
    }
}
