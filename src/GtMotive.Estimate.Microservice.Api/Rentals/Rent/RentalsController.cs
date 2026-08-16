/*
 * Aviso de propiedad intelectual
 *
 * Este repositorio se ha creado exclusivamente como prueba tÃ©cnica para Capitole.
 * Salvo los componentes de terceros y los derechos que pudieran haberse cedido
 * expresamente por contrato, el cÃ³digo y la documentaciÃ³n originales contenidos en
 * Ã©l son propiedad de su autor. No se autoriza su copia, reproducciÃ³n, modificaciÃ³n,
 * distribuciÃ³n, publicaciÃ³n ni explotaciÃ³n, total o parcial, sin consentimiento
 * previo y por escrito del titular de los derechos. El titular se reserva el
 * ejercicio de las acciones legales que correspondan frente a cualquier uso no
 * autorizado.
 */

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
