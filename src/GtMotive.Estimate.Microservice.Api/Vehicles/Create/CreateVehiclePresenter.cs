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
