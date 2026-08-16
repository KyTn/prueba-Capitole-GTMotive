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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals.Events;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Rentals.Return;

public sealed class ReturnVehicleHandler(
    ReturnVehicleUseCase useCase,
    IBusFactory busFactory,
    ITelemetry telemetry) : IRequestHandler<ReturnVehicleCommand, ReturnVehicleResult>
{
    public async Task<ReturnVehicleResult> Handle(
        ReturnVehicleCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var stopwatch = Stopwatch.StartNew();
        var outcome = "error";
        try
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);
            outcome = result.Type == ReturnVehicleResultType.Returned ? "success" : "rejected";
            if (result.Type == ReturnVehicleResultType.Returned)
            {
                var rental = result.Rental;
                var domainEvent = new VehicleReturned(
                    rental.Id,
                    rental.VehicleId,
                    rental.PersonId,
                    rental.EndedAt.Value);
                await busFactory.GetClient(domainEvent.GetType()).Send(domainEvent);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            outcome = "cancelled";
            throw;
        }
        finally
        {
            stopwatch.Stop();
            UseCaseTelemetry.Track(telemetry, "ReturnVehicle", outcome, stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
