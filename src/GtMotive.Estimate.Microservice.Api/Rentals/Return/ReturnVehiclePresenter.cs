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

using GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Rentals.Return;

internal static class ReturnVehiclePresenter
{
    public static IActionResult Present(ReturnVehicleResult result)
    {
        if (result.Type == ReturnVehicleResultType.Returned)
        {
            var rental = result.Rental;
            return new OkObjectResult(
                new ReturnVehicleResponse(
                    rental.Id,
                    rental.PersonId,
                    rental.VehicleId,
                    rental.StartedAt,
                    rental.EndedAt.Value,
                    rental.Status));
        }

        var status = result.Type switch
        {
            ReturnVehicleResultType.PersonNotFound or ReturnVehicleResultType.VehicleNotFound =>
                StatusCodes.Status404NotFound,
            ReturnVehicleResultType.VehicleNotRented or
                ReturnVehicleResultType.RentalOwnershipConflict or
                ReturnVehicleResultType.RentalAlreadyReturned =>
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
