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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Vehicles.List;

public sealed class ListVehiclesUseCase(
    IVehicleRepository repository,
    IAppLogger<ListVehiclesUseCase> logger) : IUseCase<ListVehiclesQuery>
{
    async Task IUseCase<ListVehiclesQuery>.Execute(ListVehiclesQuery input)
    {
        await ExecuteAsync(input, CancellationToken.None);
    }

    public Task<ListVehiclesResult> ExecuteAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(new ListVehiclesQuery(), cancellationToken);

    public async Task<ListVehiclesResult> ExecuteAsync(
        ListVehiclesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var vehicles = await repository.GetAllAsync(cancellationToken);
        var result = vehicles
            .Select(vehicle => new VehicleDto(
                vehicle.Id,
                vehicle.RegistrationNumber.Value,
                vehicle.Brand,
                vehicle.Model,
                vehicle.ManufactureDate))
            .ToArray();

        logger.LogInformation("{VehicleCount} vehicles listed.", result.Length);
        return new ListVehiclesResult(result);
    }
}
